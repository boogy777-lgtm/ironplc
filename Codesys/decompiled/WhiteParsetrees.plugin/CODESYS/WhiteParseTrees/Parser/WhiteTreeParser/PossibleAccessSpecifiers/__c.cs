using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using CODESYS.WhiteParseTrees.Nodes;
using CODESYS.WhiteParseTrees.Nodes.Expressions;
using CODESYS.WhiteParseTrees.Nodes.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;
using CODESYS.WhiteParseTrees.Nodes.Tokens;
using CODESYS.WhiteParseTrees.WhiteParseTrees.Extensions;

namespace CODESYS.WhiteParseTrees.Parser
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteTreeParser
	{
		[System.Runtime.CompilerServices.NullableContext(0)]
		internal struct PossibleAccessSpecifiers
		{
			private bool SeenVisibilityModifier
			{
				[IsReadOnly]
				get;
				set; }

			private bool SeenOverload
			{
				[IsReadOnly]
				get;
				set; }

			private bool SeenOverride
			{
				[IsReadOnly]
				get;
				set; }

			private bool SeenAbstractOrFinal
			{
				[IsReadOnly]
				get;
				set; }

			public void CheckNextAccessSpecifierToken(WhiteTokenType tt, out bool ok, out bool done)
			{
				done = false;
				if (IsVisibilityModifier(tt))
				{
					ok = !SeenVisibilityModifier;
					SeenVisibilityModifier = true;
					return;
				}
				SeenVisibilityModifier = true;
				switch (tt)
				{
				case WhiteTokenType.Overload:
					ok = !SeenOverload;
					SeenOverload = true;
					break;
				case WhiteTokenType.Override:
					ok = !SeenOverride;
					SeenOverride = true;
					break;
				case WhiteTokenType.Abstract:
				case WhiteTokenType.Final:
					ok = !SeenAbstractOrFinal;
					SeenAbstractOrFinal = true;
					break;
				default:
					ok = true;
					done = true;
					break;
				}
			}

			[System.Runtime.CompilerServices.NullableContext(1)]
			public IEnumerable<WhiteTokenType> GetExpectedTokenTypes()
			{
				IEnumerable<WhiteTokenType> enumerable = _accessSpecifierTokenTypes.AsEnumerable();
				if (SeenVisibilityModifier)
				{
					enumerable = enumerable.Where((WhiteTokenType t) => !IsVisibilityModifier(t));
				}
				if (SeenAbstractOrFinal)
				{
					enumerable = enumerable.Where((WhiteTokenType t) => t != WhiteTokenType.Final && t != WhiteTokenType.Abstract);
				}
				if (SeenOverload)
				{
					enumerable = enumerable.Where((WhiteTokenType t) => t != WhiteTokenType.Overload);
				}
				if (SeenOverride)
				{
					enumerable = enumerable.Where((WhiteTokenType t) => t != WhiteTokenType.Override);
				}
				return enumerable;
			}
		}

		private static readonly WhiteTokenType[] _accessSpecifierTokenTypes = new WhiteTokenType[8]
		{
			WhiteTokenType.Public,
			WhiteTokenType.Private,
			WhiteTokenType.Protected,
			WhiteTokenType.Internal,
			WhiteTokenType.Overload,
			WhiteTokenType.Override,
			WhiteTokenType.Abstract,
			WhiteTokenType.Final
		};

		private TokenControl TokenControl { get; }

		private IWhiteExpression ParseFunctionCall(IWhiteExpression callee)
		{
			ILeftParenthesisToken leftParenthesis = TokenControl.Next<ILeftParenthesisToken>();
			IList<IWhiteExpression> list = new List<IWhiteExpression>();
			IRightParenthesisToken rightParenthesis;
			while (true)
			{
				IWhiteToken whiteToken = TokenControl.LookAhead1();
				ICommaToken commaToken = null;
				if (whiteToken is IRightParenthesisToken)
				{
					rightParenthesis = TokenControl.Next<IRightParenthesisToken>();
					break;
				}
				if (whiteToken is ICommaToken)
				{
					commaToken = TokenControl.Next<ICommaToken>();
					whiteToken = TokenControl.LookAhead1();
					if (whiteToken is IRightParenthesisToken)
					{
						rightParenthesis = TokenControl.Next<IRightParenthesisToken>();
						list.Add(new WhiteLeadByCommaExpression(commaToken, new WhiteEmptyExpression()));
						break;
					}
				}
				IWhiteExpression whiteExpression = ParseCallAssignExp();
				if (commaToken != null)
				{
					whiteExpression = new WhiteLeadByCommaExpression(commaToken, whiteExpression);
				}
				list.Add(whiteExpression);
			}
			return new WhiteCallExpression(callee, leftParenthesis, list, rightParenthesis);
		}

		private IWhiteExpression ParseCaseLabelExpression()
		{
			IWhiteExpression whiteExpression = ParseSTOperand();
			if (TokenControl.LookAhead1() is IRangeToken)
			{
				IRangeToken range = TokenControl.Next<IRangeToken>();
				IWhiteExpression high = ParseSTOperand();
				return new WhiteRangeExpression(whiteExpression, range, high);
			}
			return whiteExpression;
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private IWhiteCaseLabelStatement ParseCaseLabelStatement()
		{
			int currentTokenIndex = TokenControl.CurrentTokenIndex;
			IWhiteExpression item = ParseCaseLabelExpression();
			List<IWhiteExpression> list = new List<IWhiteExpression> { item };
			IWhiteToken whiteToken;
			while (true)
			{
				whiteToken = TokenControl.Next();
				if (!(whiteToken is ICommaToken comma))
				{
					break;
				}
				IWhiteExpression expression = ParseCaseLabelExpression();
				list.Add(new WhiteLeadByCommaExpression(comma, expression));
			}
			if (whiteToken is IColonToken colon)
			{
				return new WhiteCaseLabelStatement(list, colon);
			}
			TokenControl.CurrentTokenIndex = currentTokenIndex;
			return null;
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private bool TryParseCaseLabelStatement([NotNullWhen(true)] out IWhiteCaseLabelStatement caseLabelStatement)
		{
			caseLabelStatement = null;
			switch (TokenControl.LookAhead1().Type)
			{
			case WhiteTokenType.Identifier:
			case WhiteTokenType.Integer:
				caseLabelStatement = ParseCaseLabelStatement();
				break;
			case WhiteTokenType.Plus:
			case WhiteTokenType.Minus:
			case WhiteTokenType.__SystemScope:
			case WhiteTokenType.__PoolScope:
			case WhiteTokenType.__CurrentTask:
				caseLabelStatement = ParseCaseLabelStatement();
				break;
			}
			return caseLabelStatement != null;
		}

		private IWhiteStatement ParseCaseStatement()
		{
			ICaseToken @case = TokenControl.Next<ICaseToken>();
			IWhiteExpression @switch = ParseAssignExp();
			IOfToken of = TokenControl.Next<IOfToken>();
			if (!TryParseCaseLabelStatement(out var caseLabelStatement))
			{
				IWhiteToken whiteToken = TokenControl.LookAhead1();
				string message = WhiteParserMessages.UnexpectedToken(whiteToken, typeof(IWhiteCaseLabelStatement));
				throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
			}
			List<IWhiteCase> list = new List<IWhiteCase>();
			WhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			IElseToken elseToken = null;
			WhiteSequenceStatement @else = null;
			while (true)
			{
				if (TryParseCaseLabelStatement(out var caseLabelStatement2))
				{
					if (elseToken != null)
					{
						whiteSequenceStatement.Add(caseLabelStatement2);
						continue;
					}
					list.Add(new WhiteCase(caseLabelStatement, whiteSequenceStatement));
					caseLabelStatement = caseLabelStatement2;
					whiteSequenceStatement = new WhiteSequenceStatement();
					continue;
				}
				if (TokenControl.TryNext<IEndCaseToken>(out var token))
				{
					if (elseToken != null)
					{
						@else = whiteSequenceStatement;
					}
					else
					{
						list.Add(new WhiteCase(caseLabelStatement, whiteSequenceStatement));
					}
					return new WhiteCaseStatement(@case, @switch, of, list, elseToken, @else, token);
				}
				if (TokenControl.LookAhead1() is IElseToken && elseToken == null)
				{
					elseToken = TokenControl.Next<IElseToken>();
					list.Add(new WhiteCase(caseLabelStatement, whiteSequenceStatement));
					whiteSequenceStatement = new WhiteSequenceStatement();
					@else = whiteSequenceStatement;
					continue;
				}
				IWhiteStatement whiteStatement = TryParseOneStatement();
				if (whiteStatement == null || whiteStatement is IWhiteFinalStatement)
				{
					break;
				}
				whiteSequenceStatement.Add(whiteStatement);
			}
			string message2 = WhiteParserMessages.ParsingStatementFailed();
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message2, TokenControl.StartStatementToken);
		}

		public WhiteTreeParser(TokenControl tokenControl)
		{
			TokenControl = tokenControl;
		}

		public static IWhiteTreeInformation ParseStImplementationWithSourcePositions(string stInput)
		{
			SourcePositionMap sourcePositionMap;
			return new WhiteTreeInformation(new WhiteTreeParser(new TokenControl(TokenStream.ReadTokenStream(stInput, out sourcePositionMap))).ParseST(bResynchroniseOnNullStatement: true), sourcePositionMap);
		}

		public static IWhiteSequenceStatement ParseStImplementation(string stInput)
		{
			return new WhiteTreeParser(new TokenControl(TokenStream.ReadTokenStream(stInput))).ParseST(bResynchroniseOnNullStatement: true);
		}

		public static IWhiteSequenceStatement ParseStatement(string stInput)
		{
			return new WhiteTreeParser(new TokenControl(TokenStream.ReadTokenStream(stInput))).ParseST(bResynchroniseOnNullStatement: true);
		}

		public static IWhiteExpression ParseExpression(string stInput)
		{
			return new WhiteTreeParser(new TokenControl(TokenStream.ReadTokenStream(stInput))).ParseAssignExp();
		}

		public static IWhitePOUSyntax[] ParsePOUSyntax(string stInput)
		{
			return new WhiteTreeParser(new TokenControl(TokenStream.ReadTokenStream(stInput))).ParsePOUs();
		}

		private IWhiteSequenceStatement ParseST(bool bResynchroniseOnNullStatement = false)
		{
			IWhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			while (true)
			{
				try
				{
					IWhiteStatement whiteStatement = TryParseOneStatement();
					if (whiteStatement == null)
					{
						if (bResynchroniseOnNullStatement)
						{
							string message = WhiteParserMessages.UnexpectedEndOfInput();
							throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, TokenControl.LastToken);
						}
						return whiteSequenceStatement;
					}
					whiteSequenceStatement.Add(whiteStatement);
					if (!(whiteStatement is IWhiteFinalStatement))
					{
						continue;
					}
					return whiteSequenceStatement;
				}
				catch (ResynchronizeException resynchronizeException)
				{
					whiteSequenceStatement.Add(ParseErrorUntilEndOfStatement(resynchronizeException));
				}
			}
		}

		private IList<IWhiteToken> ParseTokensForError(int startIndex, bool useStatementStart, Func<IWhiteToken, bool> terminationCondition)
		{
			TokenControl.CurrentTokenIndex = startIndex;
			List<IWhiteToken> list = new List<IWhiteToken>();
			while (TokenControl.LookAhead1(useStatementStart).Type != WhiteTokenType.End)
			{
				IWhiteToken whiteToken = TokenControl.Next(useStatementStart);
				if (!(whiteToken is INonSyntacticToken) || !TokenControl.LookAhead1().LeadingTokensRToL().Contains(whiteToken))
				{
					list.Add(whiteToken);
				}
				if (terminationCondition(whiteToken))
				{
					break;
				}
			}
			return list;
		}

		private IWhiteErrorStatement ParseErrorFromTokenRange(int startIndex, int endIndex, bool useStatementStart, ResynchronizeException resynchronizeException)
		{
			return ParseErrorFromTokenRange(startIndex, endIndex, useStatementStart, resynchronizeException.Message, resynchronizeException.ErrorToken);
		}

		private IWhiteErrorStatement ParseErrorFromTokenRange(int startIndex, int endIndex, bool useStatementStart, string errorMessage, [System.Runtime.CompilerServices.Nullable(2)] IWhiteToken errorToken)
		{
			IList<IWhiteToken> list = ParseTokensForError(startIndex, useStatementStart, (IWhiteToken _) => TokenControl.CurrentTokenIndex >= endIndex);
			if (!list.Any())
			{
				string message = WhiteParserMessages.InternalError("Could not parse error from token range. No tokens provided.");
				throw new ResynchronizeException(startIndex, message, errorToken);
			}
			return new WhiteErrorStatement(list, errorMessage, errorToken);
		}

		private IWhiteErrorStatement ParseErrorUntil(int startIndex, bool useStatementStart, Func<IWhiteToken, bool> terminationCondition, [System.Runtime.CompilerServices.Nullable(2)] string errorMessage, [System.Runtime.CompilerServices.Nullable(2)] IWhiteToken errorToken)
		{
			return new WhiteErrorStatement(ParseTokensForError(startIndex, useStatementStart, terminationCondition), errorMessage, errorToken);
		}

		private IWhiteErrorStatement ParseErrorUntil(int startIndex, bool useStatementStart, Func<IWhiteToken, bool> terminationCondition, ResynchronizeException resynchronizeException)
		{
			return ParseErrorUntil(startIndex, useStatementStart, terminationCondition, resynchronizeException.Message, resynchronizeException.ErrorToken);
		}

		private IWhiteErrorStatement ParseErrorUntilTokenTypeOrEnd(ResynchronizeException resynchronizeException, WhiteTokenType endToken)
		{
			return ParseErrorUntil(resynchronizeException.StartStatementTokenIndex, useStatementStart: true, (IWhiteToken t) => IsResynchronizeToken(t) || t.Type == endToken, resynchronizeException.Message, resynchronizeException.ErrorToken);
		}

		private IWhiteErrorStatement ParseErrorUntilEndOfStatement(ResynchronizeException resynchronizeException)
		{
			return ParseErrorUntil(resynchronizeException.StartStatementTokenIndex, useStatementStart: true, IsEndOfStatementToken, resynchronizeException.Message, resynchronizeException.ErrorToken);
		}

		private bool IsEndOfStatementToken(IWhiteToken token)
		{
			switch (token.Type)
			{
			case WhiteTokenType.End:
			case WhiteTokenType.EndAction:
			case WhiteTokenType.EndCase:
			case WhiteTokenType.EndFor:
			case WhiteTokenType.EndFunction:
			case WhiteTokenType.EndFunctionBlock:
			case WhiteTokenType.EndIf:
			case WhiteTokenType.EndProgram:
			case WhiteTokenType.EndRepeat:
			case WhiteTokenType.EndType:
			case WhiteTokenType.EndVar:
			case WhiteTokenType.EndWhile:
			case WhiteTokenType.Semicolon:
			case WhiteTokenType.__EndTry:
			case WhiteTokenType.EndMethod:
			case WhiteTokenType.EndProperty:
			case WhiteTokenType.EndInterface:
			case WhiteTokenType.EndNamespace:
			case WhiteTokenType.EndTransition:
				return true;
			default:
				return false;
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private bool IsStartOfDeclarationBlockToken(IWhiteToken token)
		{
			if (token == null)
			{
				return true;
			}
			switch (token.Type)
			{
			case WhiteTokenType.End:
			case WhiteTokenType.Action:
			case WhiteTokenType.Function:
			case WhiteTokenType.FunctionBlock:
			case WhiteTokenType.Program:
			case WhiteTokenType.Var:
			case WhiteTokenType.VarAccess:
			case WhiteTokenType.VarConfig:
			case WhiteTokenType.VarExternal:
			case WhiteTokenType.VarGlobal:
			case WhiteTokenType.VarInput:
			case WhiteTokenType.VarInOut:
			case WhiteTokenType.VarOutput:
			case WhiteTokenType.VarTemp:
			case WhiteTokenType.VarStat:
			case WhiteTokenType.Method:
			case WhiteTokenType.Interface:
			case WhiteTokenType.VarInst:
			case WhiteTokenType.VarGeneric:
			case WhiteTokenType.PropertySet:
			case WhiteTokenType.PropertyGet:
			case WhiteTokenType.Namespace:
			case WhiteTokenType.Transition:
				return true;
			default:
				return false;
			}
		}

		private WhiteTokenType? GetEndTokenForStatementStart(IWhiteToken startToken)
		{
			switch (startToken.Type)
			{
			case WhiteTokenType.__Try:
				return WhiteTokenType.__EndTry;
			case WhiteTokenType.Program:
				return WhiteTokenType.EndProgram;
			case WhiteTokenType.FunctionBlock:
				return WhiteTokenType.EndFunctionBlock;
			case WhiteTokenType.Interface:
				return WhiteTokenType.EndInterface;
			case WhiteTokenType.Function:
				return WhiteTokenType.EndFunction;
			case WhiteTokenType.Action:
				return WhiteTokenType.EndAction;
			case WhiteTokenType.Transition:
				return WhiteTokenType.EndTransition;
			case WhiteTokenType.PropertySet:
			case WhiteTokenType.PropertyGet:
				return WhiteTokenType.EndProperty;
			case WhiteTokenType.Case:
				return WhiteTokenType.EndCase;
			case WhiteTokenType.For:
				return WhiteTokenType.EndFor;
			case WhiteTokenType.If:
				return WhiteTokenType.EndIf;
			case WhiteTokenType.Repeat:
				return WhiteTokenType.EndRepeat;
			case WhiteTokenType.While:
				return WhiteTokenType.EndWhile;
			default:
				return null;
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private IWhiteStatement TryParseOneStatement()
		{
			WhiteTokenType? whiteTokenType = null;
			try
			{
				whiteTokenType = GetEndTokenForStatementStart(TokenControl.LookAhead1(bStatementStart: true));
				return ParseOneStatement();
			}
			catch (ResynchronizeException resynchronizeException)
			{
				if (whiteTokenType.HasValue)
				{
					return ParseErrorUntilTokenTypeOrEnd(resynchronizeException, whiteTokenType.Value);
				}
				return ParseErrorUntilEndOfStatement(resynchronizeException);
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private IWhiteStatement ParseOneStatement()
		{
			int currentTokenIndex = TokenControl.CurrentTokenIndex;
			try
			{
				TokenControl.StartStatementTokenIndex = TokenControl.CurrentTokenIndex;
				IWhiteToken whiteToken = TokenControl.LookAhead1(bStatementStart: true);
				switch (whiteToken.Type)
				{
				case WhiteTokenType.DocComment:
					return ParseDocuCommentStatement();
				case WhiteTokenType.Comment:
					return ParseCommentStatement();
				case WhiteTokenType.Pragma:
					return ParsePragmaStatement();
				case WhiteTokenType.Identifier:
					if (TokenControl.IsTokenToStartTransitionDeclaration(whiteToken))
					{
						return ParseDeclarationTransition();
					}
					if (TokenControl.IsTokenToStartNamespaceDeclaration(whiteToken))
					{
						return ParseDeclarationNamespace(new WhiteSequenceStatement());
					}
					return ParseIdentifierStatement();
				case WhiteTokenType.DirectVariable:
				{
					IWhiteExpression whiteExpression = ParseAssignExp();
					ISemicolonToken semicolon = ParseSemicolon();
					return new WhiteExpressionStatement(whiteExpression, semicolon);
				}
				case WhiteTokenType.End:
					return new WhiteFinalStatement((IEndToken)whiteToken);
				default:
				{
					if (whiteToken is IWhiteOperatorToken lookaheadToken)
					{
						return ParseOneOperatorStatement(lookaheadToken);
					}
					string message = WhiteParserMessages.UnexpectedToken(whiteToken);
					throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
				}
				}
			}
			catch (ResynchronizeException ex)
			{
				throw new ResynchronizeException(currentTokenIndex, ex.Message, ex.ErrorToken);
			}
		}

		private ISemicolonToken ParseSemicolon()
		{
			return TokenControl.Next<ISemicolonToken>();
		}

		private bool IsCallAssignmentOperator(IWhiteToken token)
		{
			return token is ICallAssignToken;
		}

		private bool IsAssignmentOperator(IWhiteToken token)
		{
			return token is IAnyAssignmentToken;
		}

		private static bool IsMulOperator(IWhiteToken token)
		{
			if (!(token is ITimesToken) && !(token is IDivideToken) && !(token is IModToken) && !(token is I__vcMulToken) && !(token is I__vcDivToken))
			{
				return token is I__vcDotToken;
			}
			return true;
		}

		private static bool IsAddOperator(IWhiteToken token)
		{
			if (!(token is IPlusToken) && !(token is IMinusToken) && !(token is I__vcAddToken))
			{
				return token is I__vcSubToken;
			}
			return true;
		}

		private IWhiteExpression ParseCallAssignExp()
		{
			IWhiteExpression whiteExpression = ParseORExp();
			IWhiteToken token = TokenControl.LookAhead1();
			if (IsCallAssignmentOperator(token))
			{
				ICallAssignToken assignment = TokenControl.Next<ICallAssignToken>();
				IWhiteToken whiteToken = TokenControl.LookAhead1();
				IWhiteExpression expRValue = ((!(whiteToken is ICommaToken) && !(whiteToken is IRightParenthesisToken)) ? ParseAssignExp() : new WhiteEmptyExpression());
				whiteExpression = new WhiteAssignmentExpression(whiteExpression, expRValue, assignment);
			}
			return whiteExpression;
		}

		internal IWhiteExpression ParseAssignExp()
		{
			return ParseAssignExp(IsAssignmentOperator);
		}

		private IWhiteExpression ParseAssignExp(Func<IWhiteToken, bool> ExpectedAssignmentOperator)
		{
			IWhiteExpression whiteExpression = ParseORExp();
			IWhiteToken arg = TokenControl.LookAhead1();
			if (ExpectedAssignmentOperator(arg))
			{
				IAnyAssignmentToken assignment = TokenControl.Next<IAnyAssignmentToken>();
				IWhiteExpression expRValue = ParseAssignExp(ExpectedAssignmentOperator);
				whiteExpression = new WhiteAssignmentExpression(whiteExpression, expRValue, assignment);
			}
			return whiteExpression;
		}

		private IWhiteExpression ParseORExp()
		{
			IWhiteExpression whiteExpression = ParseANDExp();
			IWhiteToken whiteToken = TokenControl.LookAhead1();
			while (whiteToken is IAnyOrToken)
			{
				IAnyOrToken token = TokenControl.Next<IAnyOrToken>();
				IWhiteExpression second = ParseANDExp();
				WhiteBinaryOperatorExpression whiteBinaryOperatorExpression = new WhiteBinaryOperatorExpression(whiteExpression, token, second);
				whiteToken = TokenControl.LookAhead1();
				whiteExpression = whiteBinaryOperatorExpression;
			}
			return whiteExpression;
		}

		private IWhiteExpression ParseANDExp()
		{
			IWhiteExpression whiteExpression = ParseCompareExp();
			IWhiteToken whiteToken = TokenControl.LookAhead1();
			while (whiteToken is IAnyAndToken)
			{
				IAnyAndToken token = TokenControl.Next<IAnyAndToken>();
				IWhiteExpression second = ParseCompareExp();
				WhiteBinaryOperatorExpression whiteBinaryOperatorExpression = new WhiteBinaryOperatorExpression(whiteExpression, token, second);
				whiteToken = TokenControl.LookAhead1();
				whiteExpression = whiteBinaryOperatorExpression;
			}
			return whiteExpression;
		}

		private IWhiteExpression ParseCompareExp()
		{
			IWhiteExpression whiteExpression = ParseADDExp();
			IWhiteToken whiteToken = TokenControl.LookAhead1();
			while (whiteToken is IWhiteComparisonOperatorToken)
			{
				IWhiteComparisonOperatorToken token = TokenControl.Next<IWhiteComparisonOperatorToken>();
				IWhiteExpression second = ParseADDExp();
				WhiteBinaryOperatorExpression whiteBinaryOperatorExpression = new WhiteBinaryOperatorExpression(whiteExpression, token, second);
				whiteToken = TokenControl.LookAhead1();
				whiteExpression = whiteBinaryOperatorExpression;
			}
			return whiteExpression;
		}

		private IWhiteExpression ParseADDExp()
		{
			IWhiteExpression whiteExpression = ParseMULExp();
			IWhiteToken token = TokenControl.LookAhead1();
			while (IsAddOperator(token))
			{
				IWhiteInfixOperatorToken token2 = TokenControl.Next<IWhiteInfixOperatorToken>();
				IWhiteExpression second = ParseMULExp();
				WhiteBinaryOperatorExpression whiteBinaryOperatorExpression = new WhiteBinaryOperatorExpression(whiteExpression, token2, second);
				token = TokenControl.LookAhead1();
				whiteExpression = whiteBinaryOperatorExpression;
			}
			return whiteExpression;
		}

		private IWhiteExpression ParseMULExp()
		{
			IWhiteExpression whiteExpression = ParseSTOperand();
			IWhiteToken token = TokenControl.LookAhead1();
			while (IsMulOperator(token))
			{
				IWhiteInfixOperatorToken token2 = TokenControl.Next<IWhiteInfixOperatorToken>();
				IWhiteExpression second = ParseSTOperand();
				WhiteBinaryOperatorExpression whiteBinaryOperatorExpression = new WhiteBinaryOperatorExpression(whiteExpression, token2, second);
				token = TokenControl.LookAhead1();
				whiteExpression = whiteBinaryOperatorExpression;
			}
			return whiteExpression;
		}

		public IWhiteExpression ParseSTOperand()
		{
			IWhiteToken whiteToken = TokenControl.Next();
			if (!(whiteToken is IBooleanToken boolean))
			{
				if (!(whiteToken is IDurationToken token))
				{
					if (!(whiteToken is ILDurationToken token2))
					{
						if (!(whiteToken is IDateToken token3))
						{
							if (!(whiteToken is ILDateToken token4))
							{
								if (!(whiteToken is ITimeOfDayToken token5))
								{
									if (!(whiteToken is ILTimeOfDayToken token6))
									{
										if (!(whiteToken is IDateAndTimeToken token7))
										{
											if (!(whiteToken is ILDateAndTimeToken token8))
											{
												if (!(whiteToken is ISingleByteStringToken token9))
												{
													if (!(whiteToken is IDoubleByteStringToken token10))
													{
														if (!(whiteToken is IXByteStringToken token11))
														{
															if (!(whiteToken is IRealToken realToken))
															{
																if (!(whiteToken is IIntegerToken intToken))
																{
																	if (!(whiteToken is IIdentifierToken token12))
																	{
																		if (!(whiteToken is IDirectVariableToken token13))
																		{
																			if (!(whiteToken is IPartialAccessToken token14))
																			{
																				if (!(whiteToken is ILeftBracketToken leftBracket))
																				{
																					if (!(whiteToken is IConversionToken conversionToken))
																					{
																						if (!(whiteToken is ILeftParenthesisToken leftParenthesis))
																						{
																							if (!(whiteToken is IPeriodToken period))
																							{
																								if (!(whiteToken is I__SystemScopeToken scope))
																								{
																									if (!(whiteToken is I__PoolScopeToken scope2))
																									{
																										if (!(whiteToken is I__CurrentTaskToken scope3))
																										{
																											if (!(whiteToken is IThisToken @this))
																											{
																												if (!(whiteToken is ISuperToken super))
																												{
																													if (!(whiteToken is IStructToken structToken))
																													{
																														if (!(whiteToken is IWhitePrefixedOperatorToken whitePrefixedOperatorToken))
																														{
																															if (whiteToken is IWhiteOperatorToken whiteOperatorToken)
																															{
																																switch (whiteOperatorToken.Type)
																																{
																																case WhiteTokenType.Not:
																																case WhiteTokenType.Plus:
																																case WhiteTokenType.Minus:
																																	return ParseUnaryOperatorExpression(whiteOperatorToken);
																																case WhiteTokenType.__Copy:
																																	throw new NotSupportedException();
																																}
																															}
																															string message = WhiteParserMessages.UnexpectedToken(whiteToken, "Operand");
																															throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
																														}
																														if (whitePrefixedOperatorToken.Type == WhiteTokenType.__Cast)
																														{
																															IWhitePrefixedOperatorExpression currentExpression = ParseCastExpression(whitePrefixedOperatorToken);
																															return ParseVarAccess(currentExpression);
																														}
																														return ParsePrefixOperator(whitePrefixedOperatorToken);
																													}
																													return ParseStructureInitialization(structToken);
																												}
																												IWhiteSuperExpression currentExpression2 = new WhiteSuperExpression(super);
																												return ParseVarAccess(currentExpression2);
																											}
																											IWhiteThisExpression currentExpression3 = new WhiteThisExpression(@this);
																											return ParseVarAccess(currentExpression3);
																										}
																										return ParseSpecialScopeExpression(scope3);
																									}
																									return ParseSpecialScopeExpression(scope2);
																								}
																								return ParseSpecialScopeExpression(scope);
																							}
																							return ParseGlobalScopeExpression(period);
																						}
																						return ParseParenthesizedExpression(leftParenthesis);
																					}
																					return ParseConversionExpression(conversionToken);
																				}
																				return ParseArrayInitialization(leftBracket);
																			}
																			return new WhitePartialAccessExpression(token14);
																		}
																		return new WhiteDirectVariableExpression(token13);
																	}
																	WhiteVariableExpression currentExpression4 = new WhiteVariableExpression(token12);
																	return ParseVarAccess(currentExpression4);
																}
																return new WhiteIntegerLiteralExpression(intToken);
															}
															return new WhiteRealLiteralExpression(realToken);
														}
														return new WhiteXStringLiteralExpression(token11);
													}
													return new WhiteDoubleByteStringLiteralExpression(token10);
												}
												return new WhiteSingleByteStringLiteralExpression(token9);
											}
											return new WhiteLDateAndTimeLiteralExpression(token8);
										}
										return new WhiteDateAndTimeLiteralExpression(token7);
									}
									return new WhiteLTimeOfDayLiteralExpression(token6);
								}
								return new WhiteTimeOfDayLiteralExpression(token5);
							}
							return new WhiteLDateLiteralExpression(token4);
						}
						return new WhiteDateLiteralExpression(token3);
					}
					return new WhiteLDurationLiteralExpression(token2);
				}
				return new WhiteDurationLiteralExpression(token);
			}
			return new WhiteBoolLiteralExpression(boolean);
		}

		private IWhiteExpression ParseStructureInitialization(IStructToken structToken)
		{
			ILeftParenthesisToken leftParenthesis = TokenControl.Next<ILeftParenthesisToken>();
			IWhiteExpression whiteExpression = ParseParenthesizedExpression(leftParenthesis);
			if (whiteExpression is IParenthesizedExpression parenthesizedExpression)
			{
				IEnumerable<IWhiteExpression> compoInits = new IWhiteExpression[1] { parenthesizedExpression.Expression };
				return new WhiteStructureInitializationExpression(parenthesizedExpression.LeftParenthesis, compoInits, parenthesizedExpression.RightParenthesis)
				{
					StructOp = structToken
				};
			}
			if (whiteExpression is IWhiteStructureInitialization whiteStructureInitialization)
			{
				whiteStructureInitialization.StructOp = structToken;
				return whiteStructureInitialization;
			}
			string message = WhiteParserMessages.FailedToParseDeclaration();
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, TokenControl.CurrentToken);
		}

		private IWhiteExpression ParseVarAccess(IWhiteExpression currentExpression)
		{
			switch (TokenControl.LookAhead1().Type)
			{
			case WhiteTokenType.DeRef:
				currentExpression = ParseDeRefAccess(currentExpression);
				break;
			case WhiteTokenType.LeftBracket:
				currentExpression = ParseArrayAccess(currentExpression);
				break;
			case WhiteTokenType.Period:
				currentExpression = ParseComponentAccess(currentExpression);
				break;
			case WhiteTokenType.Hash:
				currentExpression = ParseNamespaceAccess(currentExpression);
				break;
			case WhiteTokenType.LeftParenthesis:
				currentExpression = ParseFunctionCall(currentExpression);
				break;
			default:
				return currentExpression;
			}
			return ParseVarAccess(currentExpression);
		}

		private IWhiteExpression ParseNamespaceAccess(IWhiteExpression currentExpression)
		{
			IHashToken hashTag = TokenControl.Next<IHashToken>();
			IWhiteToken whiteToken = TokenControl.Next();
			if (whiteToken is IIdentifierToken token)
			{
				return new WhiteNamespaceAccessExpression(currentExpression, hashTag, new WhiteVariableExpression(token));
			}
			string message = WhiteParserMessages.UnexpectedToken(whiteToken, typeof(IIdentifierToken));
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
		}

		private IWhiteExpression ParseComponentAccess(IWhiteExpression currentExpression)
		{
			IPeriodToken period = TokenControl.Next<IPeriodToken>();
			IWhiteToken whiteToken = TokenControl.LookAhead1();
			switch (whiteToken.Type)
			{
			case WhiteTokenType.Integer:
			{
				IWhiteIntegerLiteralExpression rightExpression3 = new WhiteIntegerLiteralExpression(TokenControl.Next<IIntegerToken>());
				return new WhiteBitAccessExpression(currentExpression, period, rightExpression3);
			}
			case WhiteTokenType.Identifier:
			{
				IWhiteVariableExpression rightExpression2 = new WhiteVariableExpression(TokenControl.Next<IIdentifierToken>());
				return new WhiteCompoAccessExpression(currentExpression, period, rightExpression2);
			}
			case WhiteTokenType.PartialAccess:
			{
				WhitePartialAccessExpression rightExpression = new WhitePartialAccessExpression(TokenControl.Next<IPartialAccessToken>());
				return new WhiteCompoPartialAccessExpression(currentExpression, period, rightExpression);
			}
			default:
			{
				string message = WhiteParserMessages.UnexpectedToken(whiteToken, new WhiteTokenType[3]
				{
					WhiteTokenType.Integer,
					WhiteTokenType.Identifier,
					WhiteTokenType.PartialAccess
				});
				throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
			}
			}
		}

		private IWhiteExpression ParseArrayAccess(IWhiteExpression currentExpression)
		{
			ILeftBracketToken leftBracket = TokenControl.Next<ILeftBracketToken>();
			List<IWhiteExpression> list = new List<IWhiteExpression>();
			IWhiteExpression item = ParseAssignExp();
			list.Add(item);
			IWhiteToken whiteToken;
			while (true)
			{
				whiteToken = TokenControl.Next();
				if (!(whiteToken is ICommaToken comma))
				{
					break;
				}
				IWhiteExpression expression = ParseAssignExp();
				ILeadByCommaExpression item2 = new WhiteLeadByCommaExpression(comma, expression);
				list.Add(item2);
			}
			if (whiteToken is IRightBracketToken rightBracket)
			{
				return new WhiteIndexAccessExpression(currentExpression, leftBracket, list, rightBracket);
			}
			string message = WhiteParserMessages.UnexpectedToken(whiteToken, new WhiteTokenType[2]
			{
				WhiteTokenType.Comma,
				WhiteTokenType.RightBracket
			});
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
		}

		private IWhiteExpression ParseDeRefAccess(IWhiteExpression currentExpression)
		{
			IDeRefToken token = TokenControl.Next<IDeRefToken>();
			return new WhiteDeRefAccessExpression(currentExpression, token);
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private void ParseReturnTypeIfNextTokenColon(out IColonToken Colon, out IWhiteTypeExpression ReturnType)
		{
			Colon = null;
			ReturnType = null;
			if (TokenControl.TryNext<IColonToken>(out Colon))
			{
				TypeExpressionParser typeExpressionParser = new TypeExpressionParser(TokenControl);
				ReturnType = typeExpressionParser.ParseType();
			}
		}

		private static void DisconnectFromTrailing(INonSyntacticToken token)
		{
			IWhiteToken trailing = token.Trailing;
			if (trailing != null)
			{
				trailing.Leading = null;
			}
			token.Trailing = null;
		}

		private IWhitePOUSyntax[] ParsePOUs()
		{
			List<IWhitePOUSyntax> list = new List<IWhitePOUSyntax>();
			while (true)
			{
				IWhitePOUSyntax whitePOUSyntax = ParseOnePOUSyntaxChecked();
				if (whitePOUSyntax == null)
				{
					break;
				}
				list.Add(whitePOUSyntax);
			}
			return list.ToArray();
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private IWhitePOUSyntax ParseOnePOUSyntaxChecked()
		{
			int startIndex = 0;
			try
			{
				startIndex = TokenControl.CurrentTokenIndex;
				return ParseOnePOUSyntax();
			}
			catch (ResynchronizeException resynchronizeException)
			{
				return new WhiteErrorPou(ParseErrorUntil(startIndex, useStatementStart: true, IsResynchronizeToken, resynchronizeException));
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private IWhitePOUSyntax ParseOnePOUSyntax()
		{
			if (TokenControl.LookAhead1(bStatementStart: true).Type == WhiteTokenType.End)
			{
				return null;
			}
			IWhiteSequenceStatement beforeDeclaration = ParseBeforeDeclaration();
			return ParseOnePOUDeclaration(beforeDeclaration);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		private IWhitePOUSyntax ParseOnePOUDeclaration(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteToken whiteToken = TokenControl.LookAhead1(bStatementStart: true);
			switch (whiteToken.Type)
			{
			case WhiteTokenType.Function:
				return ParseFunction(beforeDeclaration);
			case WhiteTokenType.FunctionBlock:
				return ParseFunctionBlock(beforeDeclaration);
			case WhiteTokenType.Program:
				return ParseProgram(beforeDeclaration);
			case WhiteTokenType.Type:
				return ParseDeclarationDUT(beforeDeclaration);
			case WhiteTokenType.Namespace:
				return ParseDeclarationNamespace(beforeDeclaration);
			case WhiteTokenType.Interface:
				return ParseInterface(beforeDeclaration);
			case WhiteTokenType.Method:
				return ParseMethod(beforeDeclaration);
			case WhiteTokenType.PropertyGet:
				return ParsePropertyAccessor<IPropertyGetToken2>(beforeDeclaration);
			case WhiteTokenType.PropertySet:
				return ParsePropertyAccessor<IPropertySetToken2>(beforeDeclaration);
			case WhiteTokenType.End:
				return null;
			case WhiteTokenType.Identifier:
				if (TokenControl.IsTokenToStartTransitionDeclaration(whiteToken))
				{
					return ParseTransition(beforeDeclaration);
				}
				if (TokenControl.IsTokenToStartNamespaceDeclaration(whiteToken))
				{
					return ParseDeclarationNamespace(beforeDeclaration);
				}
				break;
			}
			string message = WhiteParserMessages.UnexpectedToken(whiteToken);
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
		}

		private IWhitePOUSyntax ParseOneSubPOU()
		{
			IWhiteSequenceStatement beforeDeclaration = ParseBeforeDeclaration();
			return ParseOneSubPOUDeclaration(beforeDeclaration);
		}

		private IWhitePOUSyntax ParseOneSubPOUChecked()
		{
			int startIndex = 0;
			try
			{
				startIndex = TokenControl.CurrentTokenIndex;
				return ParseOneSubPOU();
			}
			catch (ResynchronizeException resynchronizeException)
			{
				return new WhiteErrorPou(ParseErrorUntil(startIndex, useStatementStart: true, IsResynchronizeToken, resynchronizeException));
			}
		}

		private IWhitePOUSyntax ParseOneSubPOUDeclaration(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteToken whiteToken = TokenControl.LookAhead1(bStatementStart: true);
			switch (whiteToken.Type)
			{
			case WhiteTokenType.Method:
				return ParseMethod(beforeDeclaration);
			case WhiteTokenType.PropertyGet:
				return ParsePropertyAccessor<IPropertyGetToken2>(beforeDeclaration);
			case WhiteTokenType.PropertySet:
				return ParsePropertyAccessor<IPropertySetToken2>(beforeDeclaration);
			case WhiteTokenType.Action:
				return ParseAction(beforeDeclaration);
			case WhiteTokenType.Transition:
				return ParseTransition(beforeDeclaration);
			case WhiteTokenType.Identifier:
				return TryParseContextualDeclaration(beforeDeclaration, whiteToken);
			default:
			{
				string message = WhiteParserMessages.UnexpectedToken(whiteToken, new WhiteTokenType[4]
				{
					WhiteTokenType.Method,
					WhiteTokenType.PropertyGet,
					WhiteTokenType.PropertySet,
					WhiteTokenType.Action
				});
				throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
			}
			}
		}

		private IWhitePOUSyntax TryParseContextualDeclaration(IWhiteSequenceStatement beforeDeclaration, IWhiteToken next)
		{
			if (TokenControl.IsTokenToStartTransitionDeclaration(next))
			{
				return ParseTransition(beforeDeclaration);
			}
			if (TokenControl.IsTokenToStartNamespaceDeclaration(next))
			{
				return ParseDeclarationNamespace(beforeDeclaration);
			}
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex);
		}

		private IWhiteFunctionBlock ParseFunctionBlock(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteFunctionBlockDeclarationStatement whiteFunctionBlockDeclarationStatement = ParseDeclarationFunctionBlock();
			IWhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			MoveTrailingPragmasAndComments(whiteFunctionBlockDeclarationStatement.Declarations, whiteSequenceStatement);
			ParseImplementationAndSubPOUs(whiteSequenceStatement);
			IEndFunctionBlockToken2 endPOUToken = TokenControl.Next<IEndFunctionBlockToken2>();
			return new WhiteFunctionBlock(beforeDeclaration, whiteFunctionBlockDeclarationStatement, whiteSequenceStatement, endPOUToken);
		}

		private void MoveTrailingPragmasAndComments(IWhiteSequenceStatement seqImpl, IWhitePOUSyntax[] subPOUs)
		{
			if (subPOUs.Any())
			{
				MoveTrailingPragmasAndComments(seqImpl, subPOUs[0]);
			}
		}

		private void MoveTrailingPragmasAndComments(IWhiteSequenceStatement seqImpl, IWhitePOUSyntax subPOU)
		{
			if (subPOU is IWhitePOU whitePOU)
			{
				MoveTrailingPragmasAndComments(seqImpl, whitePOU.BeforeDeclarationStatements);
			}
		}

		private void MoveTrailingPragmasAndComments(IWhiteSequenceStatement seqFrom, IWhiteSequenceStatement seqTo)
		{
			while (seqFrom.Any())
			{
				IWhiteStatement whiteStatement = seqFrom.Last();
				if (whiteStatement != null && (whiteStatement is IWhiteCommentStatement || whiteStatement is IWhiteDocuCommentStatement || whiteStatement is IWhitePragmaStatement))
				{
					seqFrom.Remove(whiteStatement);
					seqTo.Insert(0, whiteStatement);
					continue;
				}
				break;
			}
		}

		private T ParseOptionalEndBlockToken<[System.Runtime.CompilerServices.Nullable(0)] T>(Operator @operator) where T : IWhiteToken
		{
			if (TokenControl.LookAhead1().Type == WhiteTokenType.End)
			{
				return (T)OperatorTokenFactory.CreateOperatorTokenByOperator("", @operator);
			}
			return TokenControl.Next<T>();
		}

		private IWhiteProgram ParseProgram(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteProgramDeclarationStatement whiteProgramDeclarationStatement = ParseDeclarationProgram();
			IWhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			MoveTrailingPragmasAndComments(whiteProgramDeclarationStatement.Declarations, whiteSequenceStatement);
			ParseImplementationAndSubPOUs(whiteSequenceStatement);
			IEndProgramToken2 endPOUToken = ParseOptionalEndBlockToken<IEndProgramToken2>(Operator.EndProgram);
			return new WhiteProgram(beforeDeclaration, whiteProgramDeclarationStatement, whiteSequenceStatement, endPOUToken);
		}

		private IWhiteInterface ParseInterface(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteInterfaceDeclarationStatement3 declaration = ParseDeclarationInterface();
			IWhitePOUSyntax[] subPOUs = ParseSubPOUs();
			IEndInterfaceToken2 endPOUToken = ParseOptionalEndBlockToken<IEndInterfaceToken2>(Operator.EndInterface);
			return new WhiteInterface(beforeDeclaration, declaration, subPOUs, endPOUToken);
		}

		private IWhiteFunction ParseFunction(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteFunctionDeclarationStatement declaration = ParseDeclarationFunction();
			IWhiteSequenceStatement implementation = ParseImplementation();
			IEndFunctionToken2 endPOUToken = ParseOptionalEndBlockToken<IEndFunctionToken2>(Operator.EndFunction);
			return new WhiteFunction(beforeDeclaration, declaration, implementation, endPOUToken);
		}

		private IWhiteMethod ParseMethod(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteMethodDeclarationStatement declaration = ParseDeclarationMethod();
			IWhiteSequenceStatement implementation = ParseImplementation();
			IEndMethodToken2 endPOUToken = ParseOptionalEndBlockToken<IEndMethodToken2>(Operator.EndMethod);
			return new WhiteMethod(beforeDeclaration, declaration, implementation, endPOUToken);
		}

		private IWhiteAction ParseAction(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteActionDeclarationStatement declaration = ParseDeclarationAction();
			IWhiteSequenceStatement implementation = ParseImplementation();
			IEndActionToken2 endPOUToken = ParseOptionalEndBlockToken<IEndActionToken2>(Operator.EndAction);
			return new WhiteAction(beforeDeclaration, declaration, implementation, endPOUToken);
		}

		private IWhiteTransition ParseTransition(IWhiteSequenceStatement beforeDeclaration)
		{
			IWhiteTransitionDeclarationStatement declaration = ParseDeclarationTransition();
			IWhiteSequenceStatement implementation = ParseImplementation();
			IEndTransitionToken endPOUToken = ParseOptionalEndBlockToken<IEndTransitionToken>(Operator.EndTransition);
			return new WhiteTransition(beforeDeclaration, declaration, implementation, endPOUToken);
		}

		private IWhiteSequenceStatement ParseBeforeDeclaration()
		{
			IWhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			while (IsBeforeDeclarationToken(TokenControl.LookAhead1(bStatementStart: true)))
			{
				IWhiteStatement whiteStatement = TryParseOneStatement();
				if (whiteStatement == null)
				{
					return whiteSequenceStatement;
				}
				whiteSequenceStatement.Add(whiteStatement);
			}
			return whiteSequenceStatement;
		}

		private IWhitePOUSyntax[] ParseSubPOUs()
		{
			List<IWhitePOUSyntax> list = new List<IWhitePOUSyntax>();
			while (IsStartOfSubPOUToken(TokenControl.LookAhead1()))
			{
				list.Add(ParseOneSubPOUChecked());
			}
			return list.ToArray();
		}

		private bool IsBeforeDeclarationToken(IWhiteToken lookAhead1)
		{
			WhiteTokenType type = lookAhead1.Type;
			if ((uint)(type - 2) <= 2u)
			{
				return true;
			}
			return false;
		}

		private IWhiteSequenceStatement ParseImplementation()
		{
			IWhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			while (IsImplementationToken(TokenControl.LookAhead1(bStatementStart: true)))
			{
				IWhiteStatement whiteStatement = TryParseOneStatement();
				if (whiteStatement == null)
				{
					return whiteSequenceStatement;
				}
				whiteSequenceStatement.Add(whiteStatement);
			}
			return whiteSequenceStatement;
		}

		private void ParseImplementationAndSubPOUs(IWhiteSequenceStatement sequenceStatement)
		{
			while (true)
			{
				if (IsImplementationToken(TokenControl.LookAhead1(bStatementStart: true)))
				{
					IWhiteStatement whiteStatement = TryParseOneStatement();
					if (whiteStatement == null)
					{
						break;
					}
					sequenceStatement.Add(whiteStatement);
					continue;
				}
				if (IsStartOfSubPOUToken(TokenControl.LookAhead1(bStatementStart: true)))
				{
					IWhitePOUSyntax whitePOUSyntax = ParseOneSubPOUChecked();
					MoveTrailingPragmasAndComments(sequenceStatement, whitePOUSyntax);
					sequenceStatement.Add(whitePOUSyntax);
					continue;
				}
				break;
			}
		}

		private bool IsImplementationToken(IWhiteToken lookAhead1)
		{
			if (!IsBeforeDeclarationToken(lookAhead1))
			{
				if (!IsStartOfSubPOUToken(lookAhead1))
				{
					return !IsEndOfPOUToken(lookAhead1);
				}
				return false;
			}
			return true;
		}

		private bool IsStartOfSubPOUToken(IWhiteToken lookAhead1)
		{
			switch (lookAhead1.Type)
			{
			case WhiteTokenType.Identifier:
				if (!TokenControl.IsTokenToStartTransitionDeclaration(lookAhead1))
				{
					return TokenControl.IsTokenToStartNamespaceDeclaration(lookAhead1);
				}
				return true;
			case WhiteTokenType.Action:
			case WhiteTokenType.Method:
			case WhiteTokenType.Property:
			case WhiteTokenType.PropertySet:
			case WhiteTokenType.PropertyGet:
			case WhiteTokenType.Transition:
				return true;
			default:
				return false;
			}
		}

		private bool IsResynchronizeToken(IWhiteToken token)
		{
			if (!IsEndOfPOUToken(token))
			{
				return token.Type == WhiteTokenType.End;
			}
			return true;
		}

		private bool IsEndOfPOUToken(IWhiteToken token)
		{
			switch (token.Type)
			{
			case WhiteTokenType.End:
			case WhiteTokenType.EndAction:
			case WhiteTokenType.EndFunction:
			case WhiteTokenType.EndFunctionBlock:
			case WhiteTokenType.EndProgram:
			case WhiteTokenType.EndType:
			case WhiteTokenType.EndMethod:
			case WhiteTokenType.EndProperty:
			case WhiteTokenType.EndInterface:
			case WhiteTokenType.EndNamespace:
			case WhiteTokenType.EndTransition:
				return true;
			default:
				return false;
			}
		}

		private IWhiteProgramDeclarationStatement ParseDeclarationProgram()
		{
			IProgramToken pouClass = TokenControl.Next<IProgramToken>();
			IEnumerable<IAccessSpecifierToken> access = ParseAccessSpecifier();
			IWhiteExpression nameExpression = ParsePOUName();
			IWhiteSequenceStatement declarations = ParseDeclaration();
			return new WhiteProgramDeclarationStatement(pouClass, access, nameExpression, declarations);
		}

		private IWhitePOUSyntax ParseDeclarationNamespace(IWhiteSequenceStatement beforeDeclaration)
		{
			IIdentifierToken identifierToken = TokenControl.Next<IIdentifierToken>();
			if (TokenControl.IsTokenWithTextNamespace(identifierToken))
			{
				IPouTypeToken pouTypeToken = new NamespaceToken(identifierToken.Text, Operator.Namespace);
				pouTypeToken.Leading = identifierToken.Leading;
				IEnumerable<IAccessSpecifierToken> accessSpecifier = ParseAccessSpecifier();
				IWhiteExpression nameExpression = ParsePOUName();
				IWhiteSequenceStatement whiteSequenceStatement = ParseDeclaration();
				IWhitePOUSyntax[] subPOUs = ParseSubPOUs();
				MoveTrailingPragmasAndComments(whiteSequenceStatement, subPOUs);
				IEndNamespaceToken endNamespace = ParseOptionalEndBlockToken<IEndNamespaceToken>(Operator.EndNamespace);
				return new WhiteNamespaceDeclarationStatement(beforeDeclaration, pouTypeToken, accessSpecifier, nameExpression, whiteSequenceStatement, subPOUs, endNamespace);
			}
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex);
		}

		private void ReplaceInvalidVariableDeclarationsWithErrorStatements(IWhiteSequenceStatement declarations)
		{
			int count = declarations.Count;
			for (int i = 0; i < count; i++)
			{
				if (declarations[i] is IWhiteVariableDeclarationListStatement whiteVariableDeclarationListStatement && Operator.Var != whiteVariableDeclarationListStatement.BeginVarOp.Operator)
				{
					string errorMessage = WhiteParserMessages.UnexpectedOperator(whiteVariableDeclarationListStatement.BeginVarOp.Operator, Operator.Var);
					IWhiteStatement item = new WhiteErrorStatement(new IWhiteStatement[1] { whiteVariableDeclarationListStatement }, errorMessage, TokenControl.StartStatementToken);
					declarations.RemoveAt(i);
					declarations.Insert(i, item);
				}
			}
		}

		private IWhitePropertyAccessorStatement ParsePropertyAccessor<[System.Runtime.CompilerServices.Nullable(0)] T>(IWhiteSequenceStatement seqBefore) where T : IPouTypeToken
		{
			T val = TokenControl.Next<T>();
			IEnumerable<IAccessSpecifierToken> accessSpecifier = ParseAccessSpecifier();
			IWhiteExpression nameExpression = ParsePOUName();
			ParseReturnTypeIfNextTokenColon(out var Colon, out var ReturnType);
			IWhiteSequenceStatement declarations = ParseDeclaration();
			ReplaceInvalidVariableDeclarationsWithErrorStatements(declarations);
			IWhiteSequenceStatement implementation = ParseImplementation();
			IEndPropertyToken2 endProperty = ParseOptionalEndBlockToken<IEndPropertyToken2>(Operator.EndProperty);
			WhitePropertyAccessorDeclarationStatement declaration = new WhitePropertyAccessorDeclarationStatement(val, accessSpecifier, nameExpression, declarations)
			{
				Colon = Colon,
				ReturnType = ReturnType
			};
			return new WhitePropertyAccessorStatement(seqBefore, declaration, implementation, endProperty);
		}

		private IWhiteFunctionBlockDeclarationStatement ParseDeclarationFunctionBlock()
		{
			IFunctionBlockToken pouClass = TokenControl.Next<IFunctionBlockToken>();
			IEnumerable<IAccessSpecifierToken> access = ParseAccessSpecifier();
			IWhiteExpression nameExpression = ParsePOUName();
			IWhiteSequenceStatement genericDeclarations = ParseVarGenericDeclaration();
			ParsePOUExtendsIfNextTokenExtend(out var Extends, out var ExtendsOp);
			ParsePOUImplementsIfNextTokenImplement(out var Implements, out var ImplementsOp);
			return new WhiteFunctionBlockDeclarationStatement(declarations: ParseDeclaration(), pouClass: pouClass, access: access, nameExpression: nameExpression, genericDeclarations: genericDeclarations, extendsOp: ExtendsOp, extends: Extends, implementsOp: ImplementsOp, implements: Implements);
		}

		private IWhiteFunctionDeclarationStatement ParseDeclarationFunction()
		{
			IFunctionToken pouClass = TokenControl.Next<IFunctionToken>();
			IEnumerable<IAccessSpecifierToken> access = ParseAccessSpecifier();
			IWhiteExpression nameExpression = ParsePOUName();
			ParseReturnTypeIfNextTokenColon(out var Colon, out var ReturnType);
			ISemicolonToken returnTypeTrailingSemicolon = null;
			if (TokenControl.LookAhead1() is ISemicolonToken)
			{
				returnTypeTrailingSemicolon = TokenControl.Next<ISemicolonToken>();
			}
			return new WhiteFunctionDeclarationStatement(declarations: ParseDeclaration(), pouClass: pouClass, access: access, nameExpression: nameExpression, colon: Colon, returnType: ReturnType, returnTypeTrailingSemicolon: returnTypeTrailingSemicolon);
		}

		private IWhiteMethodDeclarationStatement ParseDeclarationMethod()
		{
			IMethodToken pouClass = TokenControl.Next<IMethodToken>();
			IEnumerable<IAccessSpecifierToken> access = ParseAccessSpecifier();
			IWhiteExpression nameExpression = ParsePOUName();
			ParseReturnTypeIfNextTokenColon(out var Colon, out var ReturnType);
			return new WhiteMethodDeclarationStatement(declarations: ParseDeclaration(), pouClass: pouClass, access: access, nameExpression: nameExpression, colon: Colon, returnType: ReturnType);
		}

		private IWhiteActionDeclarationStatement ParseDeclarationAction()
		{
			IActionToken2 pouClass = TokenControl.Next<IActionToken2>();
			IEnumerable<IAccessSpecifierToken> source = ParseAccessSpecifier();
			List<IErrorToken> list = new List<IErrorToken>();
			list.AddRange(source.Select((IAccessSpecifierToken am) => new ErrorToken(am.Text)
			{
				Leading = am.Leading
			}));
			IWhiteExpression nameExpression = ParsePOUName();
			ParsePOUExtendsIfNextTokenExtend(out var Extends, out var ExtendsOp);
			ParsePOUImplementsIfNextTokenImplement(out var Implements, out var ImplementsOp);
			IEnumerable<IErrorToken> erroneousExtendsOrImplementsTokens = GetErroneousExtendsOrImplementsTokens(Extends, ExtendsOp, Implements, ImplementsOp);
			IWhiteSequenceStatement declarations = ParseDeclaration();
			return new WhiteActionDeclarationStatement(pouClass, list, nameExpression, erroneousExtendsOrImplementsTokens, declarations);
		}

		private IWhiteTransitionDeclarationStatement ParseDeclarationTransition()
		{
			IIdentifierToken identifierToken = TokenControl.Next<IIdentifierToken>();
			if (TokenControl.IsTokenWithTextTransition(identifierToken))
			{
				IPouTypeToken pouTypeToken = new TransitionToken(identifierToken.Text, Operator.Transition);
				pouTypeToken.Leading = identifierToken.Leading;
				IEnumerable<IAccessSpecifierToken> source = ParseAccessSpecifier();
				List<IErrorToken> list = new List<IErrorToken>();
				list.AddRange(source.Select((IAccessSpecifierToken am) => new ErrorToken(am.Text)
				{
					Leading = am.Leading
				}));
				IWhiteExpression nameExpression = ParsePOUName();
				ParsePOUExtendsIfNextTokenExtend(out var Extends, out var ExtendsOp);
				ParsePOUImplementsIfNextTokenImplement(out var Implements, out var ImplementsOp);
				IEnumerable<IErrorToken> erroneousExtendsOrImplementsTokens = GetErroneousExtendsOrImplementsTokens(Extends, ExtendsOp, Implements, ImplementsOp);
				IWhiteSequenceStatement declarations = ParseDeclaration();
				return new WhiteTransitionDeclarationStatement(pouTypeToken, list, nameExpression, erroneousExtendsOrImplementsTokens, declarations);
			}
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex);
		}

		private IEnumerable<IErrorToken> GetErroneousExtendsOrImplementsTokens(IList<IWhiteExpression> extends, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IList<IWhiteExpression> implements, [System.Runtime.CompilerServices.Nullable(2)] IImplementsToken implementsOp)
		{
			List<IErrorToken> list = new List<IErrorToken>();
			if (extendsOp != null)
			{
				list.Add(new ErrorToken(extendsOp.Text)
				{
					Leading = extendsOp.Leading
				});
			}
			foreach (IWhiteExpression extend in extends)
			{
				AddChildrenAsErrorTokens(extend, list);
			}
			if (implementsOp != null)
			{
				list.Add(new ErrorToken(implementsOp.Text)
				{
					Leading = implementsOp.Leading
				});
			}
			foreach (IWhiteExpression implement in implements)
			{
				AddChildrenAsErrorTokens(implement, list);
			}
			return list;
		}

		private void AddChildrenAsErrorTokens(IWhiteExpression expr, List<IErrorToken> result)
		{
			foreach (INode child in expr.GetChildren())
			{
				if (child is IWhiteToken whiteToken)
				{
					result.Add(new ErrorToken(whiteToken.Text)
					{
						Leading = whiteToken.Leading
					});
				}
				else
				{
					result.Add(new ErrorToken(child.ToString()));
				}
			}
		}

		private IWhiteInterfaceDeclarationStatement3 ParseDeclarationInterface()
		{
			IInterfaceToken pouClass = TokenControl.Next<IInterfaceToken>();
			List<IAccessSpecifierToken> access = ParseAccessSpecifier();
			IWhiteExpression nameExpression = ParsePOUName();
			ParsePOUExtendsIfNextTokenExtend(out var Extends, out var ExtendsOp);
			ParsePOUImplementsIfNextTokenImplement(out var Implements, out var ImplementsOp);
			return new WhiteInterfaceDeclarationStatement(declarations: ParseDeclaration(), pouClass: pouClass, access: access, nameExpression: nameExpression, extendsOp: ExtendsOp, extends: Extends, implementsOp: ImplementsOp, implements: Implements);
		}

		private IWhiteStatement ParseDeclarationProperty()
		{
			IPropertyToken propertyOp = TokenControl.Next<IPropertyToken>();
			IEnumerable<IAccessSpecifierToken> access = ParseAccessSpecifier();
			IWhiteExpression nameExpression = ParsePOUName();
			ParseReturnTypeIfNextTokenColon(out var Colon, out var ReturnType);
			if (Colon == null || ReturnType == null)
			{
				IWhiteToken whiteToken = TokenControl.LookAhead1();
				throw new ResynchronizeException(message: WhiteParserMessages.UnexpectedToken(whiteToken, WhiteTokenType.Colon), startStatementTokenIndex: TokenControl.StartStatementTokenIndex, errorToken: whiteToken);
			}
			return new WhitePropertyDeclarationStatement(propertyOp, access, nameExpression, Colon, ReturnType);
		}

		private IWhiteDUT ParseDeclarationDUT(IWhiteSequenceStatement seqBeforeDeclaration)
		{
			ITypeToken typeOp = TokenControl.Next<ITypeToken>();
			List<IAccessSpecifierToken> accessSpecifier = ParseAccessSpecifier();
			IWhiteExpression nameExpression = ParsePOUName();
			ParsePOUExtendsIfNextTokenExtend(out var Extends, out var ExtendsOp);
			IColonToken colon = TokenControl.Next<IColonToken>();
			IWhiteTypeDeclarationStatement whiteTypeDeclarationStatement;
			IEndTypeToken endTypeOp;
			if (TokenControl.CheckNext<IUnionToken>())
			{
				whiteTypeDeclarationStatement = ParseUnion(typeOp, accessSpecifier, nameExpression, colon);
				endTypeOp = ((IWhiteUnionDeclarationStatement2)whiteTypeDeclarationStatement).EndTypeOp;
			}
			else if (TokenControl.CheckNext<IStructToken>())
			{
				whiteTypeDeclarationStatement = ParseStruct(typeOp, accessSpecifier, nameExpression, ExtendsOp, Extends, colon);
				endTypeOp = ((IWhiteStructDeclarationStatement2)whiteTypeDeclarationStatement).EndTypeOp;
			}
			else if (TokenControl.CheckNext<ILeftParenthesisToken>())
			{
				whiteTypeDeclarationStatement = ParseEnum(typeOp, accessSpecifier, nameExpression, colon);
				endTypeOp = ((IWhiteEnumDeclarationStatement2)whiteTypeDeclarationStatement).EndTypeOp;
			}
			else
			{
				whiteTypeDeclarationStatement = ParseAlias(typeOp, accessSpecifier, nameExpression, colon);
				endTypeOp = ((IWhiteAliasDeclarationStatement2)whiteTypeDeclarationStatement).EndTypeOp;
			}
			return new WhiteDUT(seqBeforeDeclaration, whiteTypeDeclarationStatement, endTypeOp);
		}

		private IWhiteUnionDeclarationStatement2 ParseUnion(ITypeToken typeOp, List<IAccessSpecifierToken> accessSpecifier, IWhiteExpression nameExpression, IColonToken colon)
		{
			IWhiteStatement whiteStatement = ParseVariableList();
			if (whiteStatement is IWhiteErrorStatement)
			{
				string message = WhiteParserMessages.FailedToParseDeclaration();
				throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, TokenControl.StartStatementToken);
			}
			IEndTypeToken endTypeOp = ParseOptionalEndBlockToken<IEndTypeToken>(Operator.EndType);
			return new WhiteUnionDeclarationStatement(typeOp, accessSpecifier, nameExpression, colon, whiteStatement, endTypeOp);
		}

		private IWhiteStructDeclarationStatement2 ParseStruct(ITypeToken typeOp, List<IAccessSpecifierToken> accessSpecifier, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IList<IWhiteExpression> extends, IColonToken colon)
		{
			IWhiteStatement whiteStatement = ParseVariableList();
			if (whiteStatement is IWhiteErrorStatement)
			{
				string message = WhiteParserMessages.FailedToParseDeclaration();
				throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, TokenControl.StartStatementToken);
			}
			IEndTypeToken endTypeOp = ParseOptionalEndBlockToken<IEndTypeToken>(Operator.EndType);
			return new WhiteStructDeclarationStatement(typeOp, accessSpecifier, nameExpression, extendsOp, extends, colon, whiteStatement, endTypeOp);
		}

		private IWhiteAliasDeclarationStatement2 ParseAlias(ITypeToken typeOp, List<IAccessSpecifierToken> accessSpecifier, IWhiteExpression nameExpression, IColonToken colon)
		{
			IWhiteTypeExpression type = new TypeExpressionParser(TokenControl).ParseType();
			ISemicolonToken semicolon = TokenControl.Next<ISemicolonToken>();
			IEndTypeToken endTypeOp = ParseOptionalEndBlockToken<IEndTypeToken>(Operator.EndType);
			return new WhiteAliasDeclarationStatement(typeOp, accessSpecifier, nameExpression, colon, type, semicolon, endTypeOp);
		}

		private IWhiteEnumDeclarationStatement2 ParseEnum(ITypeToken typeOp, List<IAccessSpecifierToken> accessSpecifier, IWhiteExpression nameExpression, IColonToken colon)
		{
			ILeftParenthesisToken leftParenthesis = TokenControl.Next<ILeftParenthesisToken>();
			List<IWhiteExpression> list = new List<IWhiteExpression>();
			if (!TokenControl.CheckNext<IRightParenthesisToken>())
			{
				IWhiteExpression item = ParseAssignExp();
				list.Add(item);
				while (TokenControl.LookAhead1() is ICommaToken)
				{
					ICommaToken comma = TokenControl.Next<ICommaToken>();
					IWhiteExpression expression = ParseAssignExp();
					WhiteLeadByCommaExpression item2 = new WhiteLeadByCommaExpression(comma, expression);
					list.Add(item2);
				}
			}
			IRightParenthesisToken rightParenthesis = TokenControl.Next<IRightParenthesisToken>();
			IWhiteTypeExpression typeExpression = null;
			if (TokenControl.CheckNext<IWhiteSimpleTypeToken>())
			{
				typeExpression = new TypeExpressionParser(TokenControl).ParseType();
			}
			IWhiteExpression initialization = null;
			if (TokenControl.TryNext<IAssignToken>(out var token))
			{
				initialization = ParseAssignExp();
			}
			ISemicolonToken semicolon = TokenControl.Next<ISemicolonToken>();
			IEndTypeToken endTypeOp = ParseOptionalEndBlockToken<IEndTypeToken>(Operator.EndType);
			EnumerationTypeExpression enumerationTypeExpression = new EnumerationTypeExpression(leftParenthesis, list, rightParenthesis);
			return new WhiteEnumDeclarationStatement(typeOp, accessSpecifier, nameExpression, colon, enumerationTypeExpression, typeExpression, token, initialization, semicolon, endTypeOp);
		}

		private IWhiteSequenceStatement ParseDeclaration()
		{
			WhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			while (IsDeclarationToken(TokenControl.LookAhead1(bStatementStart: true)))
			{
				try
				{
					IWhiteStatement whiteStatement = ParseOneStatement();
					if (!(whiteStatement is IWhiteFinalStatement))
					{
						if (whiteStatement != null)
						{
							whiteSequenceStatement.Add(whiteStatement);
							continue;
						}
						return whiteSequenceStatement;
					}
					return whiteSequenceStatement;
				}
				catch (ResynchronizeException resynchronizeException)
				{
					whiteSequenceStatement.Add(ParseErrorUntilEndOfStatement(resynchronizeException));
				}
			}
			return whiteSequenceStatement;
		}

		private bool IsDeclarationToken(IWhiteToken lookAhead1)
		{
			switch (lookAhead1.Type)
			{
			case WhiteTokenType.Union:
			case WhiteTokenType.Var:
			case WhiteTokenType.VarConfig:
			case WhiteTokenType.VarExternal:
			case WhiteTokenType.VarGlobal:
			case WhiteTokenType.VarInput:
			case WhiteTokenType.VarInOut:
			case WhiteTokenType.VarOutput:
			case WhiteTokenType.VarTemp:
			case WhiteTokenType.VarStat:
			case WhiteTokenType.VarInst:
			case WhiteTokenType.VarGeneric:
				return true;
			default:
				return IsBeforeDeclarationToken(lookAhead1);
			}
		}

		private IWhiteSequenceStatement ParseVarGenericDeclaration()
		{
			WhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			while (true)
			{
				try
				{
					if (IsValidVarGenericDeclarationStartToken(TokenControl.LookAhead1()))
					{
						IWhiteStatement whiteStatement = ParseOneStatement();
						if (!(whiteStatement is IWhiteFinalStatement))
						{
							if (whiteStatement != null)
							{
								whiteSequenceStatement.Add(whiteStatement);
								if (!(whiteStatement is IWhiteVariableDeclarationListStatement whiteVariableDeclarationListStatement) || !(whiteVariableDeclarationListStatement.BeginVarOp is IVarGenericToken))
								{
									continue;
								}
								return whiteSequenceStatement;
							}
							return whiteSequenceStatement;
						}
						return whiteSequenceStatement;
					}
					return whiteSequenceStatement;
				}
				catch (ResynchronizeException resynchronizeException)
				{
					whiteSequenceStatement.Add(ParseErrorUntilEndOfStatement(resynchronizeException));
				}
			}
		}

		private bool IsValidVarGenericDeclarationStartToken(IWhiteToken token)
		{
			if (token is IVarGenericToken || token is IPragmaToken || token is ICommentToken || token is IDocCommentToken)
			{
				return true;
			}
			return false;
		}

		public static bool IsVisibilityModifier(WhiteTokenType tt)
		{
			if ((uint)(tt - 248) <= 3u)
			{
				return true;
			}
			return false;
		}

		private List<IAccessSpecifierToken> ParseAccessSpecifier()
		{
			List<IAccessSpecifierToken> list = new List<IAccessSpecifierToken>();
			PossibleAccessSpecifiers possibleAccessSpecifiers = default(PossibleAccessSpecifiers);
			bool done;
			do
			{
				IWhiteToken whiteToken = TokenControl.LookAhead1();
				possibleAccessSpecifiers.CheckNextAccessSpecifierToken(whiteToken.Type, out var ok, out done);
				if (!ok)
				{
					IEnumerable<WhiteTokenType> expectedTokenTypes = possibleAccessSpecifiers.GetExpectedTokenTypes();
					throw new ResynchronizeException(message: (!expectedTokenTypes.Any()) ? WhiteParserMessages.UnexpectedToken(whiteToken, WhiteTokenType.Identifier) : WhiteParserMessages.UnexpectedToken(whiteToken, expectedTokenTypes), startStatementTokenIndex: TokenControl.StartStatementTokenIndex, errorToken: whiteToken);
				}
				if (!done)
				{
					list.Add(TokenControl.Next<IAccessSpecifierToken>());
				}
			}
			while (!done);
			return list;
		}

		private IWhiteExpression ParsePOUName()
		{
			IWhiteToken whiteToken = TokenControl.Next();
			return new WhiteVariableExpression((whiteToken as IIdentifierToken) ?? throw new ResynchronizeException(message: WhiteParserMessages.UnexpectedToken(whiteToken, WhiteTokenType.Identifier), startStatementTokenIndex: TokenControl.StartStatementTokenIndex, errorToken: whiteToken));
		}

		private void ParsePOUExtendsIfNextTokenExtend(out IList<IWhiteExpression> Extends, [System.Runtime.CompilerServices.Nullable(2)] out IExtendsToken ExtendsOp)
		{
			ExtendsOp = null;
			Extends = new List<IWhiteExpression>();
			if (TokenControl.CheckNext<IExtendsToken>())
			{
				ParsePOUExtends(out Extends, out ExtendsOp);
			}
		}

		private void ParsePOUImplementsIfNextTokenImplement(out IList<IWhiteExpression> Implements, [System.Runtime.CompilerServices.Nullable(2)] out IImplementsToken ImplementsOp)
		{
			ImplementsOp = null;
			Implements = new List<IWhiteExpression>();
			if (TokenControl.CheckNext<IImplementsToken>())
			{
				ParsePOUImplements(out Implements, out ImplementsOp);
			}
		}

		private void ParsePOUImplements(out IList<IWhiteExpression> Implements, out IImplementsToken ImplementsOp)
		{
			ImplementsOp = TokenControl.Next<IImplementsToken>();
			Implements = new List<IWhiteExpression>();
			IWhiteExpression item = ParseAssignExp();
			Implements.Add(item);
			ICommaToken token;
			while (TokenControl.TryNext<ICommaToken>(out token))
			{
				IWhiteExpression expression = ParseAssignExp();
				WhiteLeadByCommaExpression item2 = new WhiteLeadByCommaExpression(token, expression);
				Implements.Add(item2);
			}
		}

		private void ParsePOUExtends(out IList<IWhiteExpression> Extends, out IExtendsToken ExtendsOp)
		{
			ExtendsOp = TokenControl.Next<IExtendsToken>();
			Extends = new List<IWhiteExpression>();
			IWhiteExpression item = ParseAssignExp();
			Extends.Add(item);
			ICommaToken token;
			while (TokenControl.TryNext<ICommaToken>(out token))
			{
				IWhiteExpression expression = ParseAssignExp();
				WhiteLeadByCommaExpression item2 = new WhiteLeadByCommaExpression(token, expression);
				Extends.Add(item2);
			}
		}

		private IWhiteStatement ParseForStatement()
		{
			IForToken @for = TokenControl.Next<IForToken>();
			if (!(ParseAssignExp() is IWhiteAssignmentExpression startExpression))
			{
				throw new ResynchronizeException(message: WhiteParserMessages.FailedToParseAssignmentExpression(), startStatementTokenIndex: TokenControl.StartStatementTokenIndex, errorToken: TokenControl.StartStatementToken);
			}
			IToToken to = TokenControl.Next<IToToken>();
			IWhiteExpression upperBound = ParseAssignExp();
			IWhiteToken whiteToken = TokenControl.Next();
			IByToken by = null;
			IWhiteExpression stepWidth = null;
			if (whiteToken is IByToken byToken)
			{
				by = byToken;
				stepWidth = ParseSTOperand();
				whiteToken = TokenControl.Next();
			}
			IDoToken @do = TokenControl.Assert<IDoToken>(whiteToken);
			IWhiteSequenceStatement controlled = ParseST();
			IEndForToken endFor = TokenControl.Next<IEndForToken>();
			return new WhiteForStatement(@for, startExpression, to, upperBound, by, stepWidth, @do, controlled, endFor);
		}

		private IWhiteElseIfStatement ParseElseIf()
		{
			IElseIfToken elseIf = TokenControl.Next<IElseIfToken>();
			IWhiteExpression condition = ParseAssignExp();
			IThenToken then = TokenControl.Next<IThenToken>();
			IWhiteSequenceStatement thenStatement = ParseST();
			return new WhiteElseIfStatement(elseIf, condition, then, thenStatement);
		}

		private IWhiteStatement ParseIf()
		{
			IIfToken @if = TokenControl.Next<IIfToken>();
			IWhiteExpression condition = ParseAssignExp();
			IThenToken then = TokenControl.Next<IThenToken>();
			IWhiteSequenceStatement thenStatement = ParseST();
			IWhiteSequenceStatement elseStatement = null;
			IWhiteToken whiteToken = TokenControl.LookAhead1();
			IElseToken @else = null;
			List<IWhiteElseIfStatement> list = new List<IWhiteElseIfStatement>();
			while (whiteToken is IElseIfToken)
			{
				IWhiteElseIfStatement item = ParseElseIf();
				list.Add(item);
				whiteToken = TokenControl.LookAhead1();
			}
			if (whiteToken is IElseToken)
			{
				@else = TokenControl.Next<IElseToken>();
				elseStatement = ParseST();
			}
			IEndIfToken endif = TokenControl.Next<IEndIfToken>();
			return new WhiteIfStatement(@if, condition, then, thenStatement, list, @else, elseStatement, endif);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteStatement ParseOneOperatorStatement(IWhiteOperatorToken lookaheadToken)
		{
			switch (lookaheadToken.Type)
			{
			case WhiteTokenType.__Copy:
			case WhiteTokenType.This:
			case WhiteTokenType.Super:
			case WhiteTokenType.Period:
			case WhiteTokenType.__Init:
			case WhiteTokenType.__QueryInterface:
			case WhiteTokenType.__QueryPointer:
			case WhiteTokenType.__Delete:
			case WhiteTokenType.__Cast:
			case WhiteTokenType.__FCall:
			case WhiteTokenType.__PropertyInfo:
			case WhiteTokenType.__Throw:
			case WhiteTokenType.__CallInitFunction:
			case WhiteTokenType.__LateCompiledExpr:
			case WhiteTokenType.__PoolScope:
			case WhiteTokenType.__MemoryBarrier:
			case WhiteTokenType.__CurrentTask:
			case WhiteTokenType.__vcStore:
			{
				IWhiteExpression whiteExpression = ParseAssignExp();
				ISemicolonToken semicolon3 = ParseSemicolon();
				return new WhiteExpressionStatement(whiteExpression, semicolon3);
			}
			case WhiteTokenType.Semicolon:
				return new WhiteEmptyStatement(ParseSemicolon());
			case WhiteTokenType.Return:
				return ParseReturnStatement();
			case WhiteTokenType.Jmp:
				return ParseJumpStatement();
			case WhiteTokenType.Exit:
			{
				IExitToken exitToken = TokenControl.Next<IExitToken>();
				ISemicolonToken semicolon2 = ParseSemicolon();
				return new WhiteExitStatement(exitToken, semicolon2);
			}
			case WhiteTokenType.Continue:
			{
				IContinueToken cContinue = TokenControl.Next<IContinueToken>();
				ISemicolonToken semicolon = ParseSemicolon();
				return new WhiteContinueStatement(cContinue, semicolon);
			}
			case WhiteTokenType.__Try:
				return ParseTryCatchStatement();
			case WhiteTokenType.If:
				return ParseIf();
			case WhiteTokenType.While:
				return ParseWhileStatement();
			case WhiteTokenType.Repeat:
				return ParseRepeatStatement();
			case WhiteTokenType.For:
				return ParseForStatement();
			case WhiteTokenType.Case:
				return ParseCaseStatement();
			case WhiteTokenType.Function:
				return ParseDeclarationFunction();
			case WhiteTokenType.Method:
				return ParseDeclarationMethod();
			case WhiteTokenType.Action:
				return ParseDeclarationAction();
			case WhiteTokenType.Transition:
				return ParseDeclarationTransition();
			case WhiteTokenType.FunctionBlock:
				return ParseDeclarationFunctionBlock();
			case WhiteTokenType.Program:
				return ParseDeclarationProgram();
			case WhiteTokenType.Interface:
				return ParseDeclarationInterface();
			case WhiteTokenType.Property:
				return ParseDeclarationProperty();
			case WhiteTokenType.Type:
				return ParseDeclarationDUT(new WhiteSequenceStatement());
			case WhiteTokenType.Union:
			case WhiteTokenType.Var:
			case WhiteTokenType.VarConfig:
			case WhiteTokenType.VarExternal:
			case WhiteTokenType.VarGlobal:
			case WhiteTokenType.VarInput:
			case WhiteTokenType.VarInOut:
			case WhiteTokenType.VarOutput:
			case WhiteTokenType.VarTemp:
			case WhiteTokenType.VarStat:
			case WhiteTokenType.VarInst:
			case WhiteTokenType.VarGeneric:
				return ParseVariableList();
			case WhiteTokenType.Namespace:
				return ParseDeclarationNamespace(new WhiteSequenceStatement());
			case WhiteTokenType.PropertyGet:
				return ParsePropertyAccessor<IPropertyGetToken2>(new WhiteSequenceStatement());
			case WhiteTokenType.PropertySet:
				return ParsePropertyAccessor<IPropertySetToken2>(new WhiteSequenceStatement());
			case WhiteTokenType.Else:
			case WhiteTokenType.Elsif:
			case WhiteTokenType.EndAction:
			case WhiteTokenType.EndCase:
			case WhiteTokenType.EndFor:
			case WhiteTokenType.EndFunction:
			case WhiteTokenType.EndFunctionBlock:
			case WhiteTokenType.EndIf:
			case WhiteTokenType.EndProgram:
			case WhiteTokenType.EndRepeat:
			case WhiteTokenType.EndStruct:
			case WhiteTokenType.EndUnion:
			case WhiteTokenType.EndType:
			case WhiteTokenType.EndVar:
			case WhiteTokenType.EndWhile:
			case WhiteTokenType.Until:
			case WhiteTokenType.__EndTry:
			case WhiteTokenType.__Catch:
			case WhiteTokenType.__Finally:
			case WhiteTokenType.EndMethod:
			case WhiteTokenType.EndProperty:
			case WhiteTokenType.EndInterface:
			case WhiteTokenType.EndNamespace:
			case WhiteTokenType.EndTransition:
				return null;
			default:
			{
				string message = WhiteParserMessages.UnexpectedToken(lookaheadToken, "OperatorStatement");
				throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, lookaheadToken);
			}
			}
		}

		[System.Runtime.CompilerServices.NullableContext(2)]
		private IWhiteExpression TryParseParenthesizedExpression()
		{
			IWhiteExpression result = null;
			if (TokenControl.LookAhead1() is ILeftParenthesisToken)
			{
				ILeftParenthesisToken leftParenthesis = TokenControl.Next<ILeftParenthesisToken>();
				result = ParseParenthesizedExpression(leftParenthesis);
			}
			return result;
		}

		private IWhiteExpression ParseParenthesizedExpression(ILeftParenthesisToken leftParenthesis)
		{
			if (TokenControl.TryNext<IRightParenthesisToken>(out var token))
			{
				return new WhiteStructureInitializationExpression(leftParenthesis, Enumerable.Empty<IWhiteExpression>(), token);
			}
			IWhiteExpression whiteExpression = ParseAssignExp();
			if (TokenControl.TryNext<IRightParenthesisToken>(out var token2))
			{
				return new WhiteParenthesizedExpression(leftParenthesis, whiteExpression, token2);
			}
			List<IWhiteExpression> list = new List<IWhiteExpression> { whiteExpression };
			ICommaToken token3;
			while (TokenControl.TryNext<ICommaToken>(out token3))
			{
				whiteExpression = ParseAssignExp();
				WhiteLeadByCommaExpression item = new WhiteLeadByCommaExpression(token3, whiteExpression);
				list.Add(item);
			}
			return new WhiteStructureInitializationExpression(leftParenthesis, list, TokenControl.Next<IRightParenthesisToken>());
		}

		private IWhiteStatement ParseReturnStatement()
		{
			IReturnToken tokenReturn = TokenControl.Next<IReturnToken>();
			IParenthesizedExpression condition = TryParseParenthesizedExpression() as IParenthesizedExpression;
			ISemicolonToken semicolon = ParseSemicolon();
			return new WhiteReturnStatement(tokenReturn, condition, semicolon);
		}

		private IWhiteStatement ParseJumpStatement()
		{
			IJumpToken tokenJump = TokenControl.Next<IJumpToken>();
			IParenthesizedExpression condition = TryParseParenthesizedExpression() as IParenthesizedExpression;
			IIdentifierToken labelToken = TokenControl.Next<IIdentifierToken>();
			ISemicolonToken semicolon = ParseSemicolon();
			return new WhiteJumpStatement(tokenJump, condition, labelToken, semicolon);
		}

		private IWhiteStatement ParsePragmaStatement()
		{
			IPragmaToken pragmaToken = (IPragmaToken)TokenControl.Next(statementStart: true);
			DisconnectFromTrailing(pragmaToken);
			if (PragmaChecker.CheckFor<IIfToken>(pragmaToken.Pragma))
			{
				return ParsePragmaIfStatement(pragmaToken);
			}
			return new WhitePragmaStatement(pragmaToken);
		}

		private IWhitePragmaIfStatement ParsePragmaIfStatement(IPragmaToken token)
		{
			IWhitePragmaStatement next;
			IWhiteSequenceStatement then = ParseUntil(PragmaElseElsifOrEndifExpected, out next);
			List<IWhitePragmaElseIfStatement> list = new List<IWhitePragmaElseIfStatement>();
			while (next != null && PragmaChecker.CheckFor<IElseIfToken>(next.PragmaToken.Pragma))
			{
				list.Add(ParsePragmaElseIf(next, out next));
			}
			IWhitePragmaStatement @else = null;
			IWhiteSequenceStatement elseStatement = null;
			if (next != null && PragmaChecker.CheckFor<IElseToken>(next.PragmaToken.Pragma))
			{
				@else = next;
				elseStatement = ParseUntil(PragmaEndifExpected, out next);
			}
			if (next != null && PragmaChecker.CheckFor<IEndIfToken>(next.PragmaToken.Pragma))
			{
				return new WhitePragmaIfStatement(new WhitePragmaStatement(token), then, list, @else, elseStatement, next);
			}
			string message = WhiteParserMessages.UnexpectedToken(token, new WhiteTokenType[2]
			{
				WhiteTokenType.Elsif,
				WhiteTokenType.EndIf
			});
			throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, token);
		}

		private IWhitePragmaElseIfStatement ParsePragmaElseIf(IWhitePragmaStatement pragmaStatement, [System.Runtime.CompilerServices.Nullable(2)] out IWhitePragmaStatement nextStatement)
		{
			IWhiteSequenceStatement elseIfStatements = ParseUntil(PragmaElseElsifOrEndifExpected, out nextStatement);
			return new WhitePragmaElseIfStatement(pragmaStatement, elseIfStatements);
		}

		private bool PragmaElseElsifOrEndifExpected(IWhiteStatement statement)
		{
			if (statement is IWhitePragmaStatement whitePragmaStatement)
			{
				if (PragmaChecker.CheckFor<IElseToken>(whitePragmaStatement.PragmaToken.Pragma))
				{
					return true;
				}
				if (PragmaChecker.CheckFor<IElseIfToken>(whitePragmaStatement.PragmaToken.Pragma))
				{
					return true;
				}
				if (PragmaChecker.CheckFor<IEndIfToken>(whitePragmaStatement.PragmaToken.Pragma))
				{
					return true;
				}
			}
			return false;
		}

		private bool PragmaEndifExpected(IWhiteStatement statement)
		{
			if (statement is IWhitePragmaStatement whitePragmaStatement && PragmaChecker.CheckFor<IEndIfToken>(whitePragmaStatement.PragmaToken.Pragma))
			{
				return true;
			}
			return false;
		}

		private IWhiteSequenceStatement ParseUntil(Func<IWhiteStatement, bool> CheckForNextPragmaStatment, [System.Runtime.CompilerServices.Nullable(2)] out IWhitePragmaStatement next)
		{
			IWhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			next = null;
			while (true)
			{
				try
				{
					IWhiteStatement whiteStatement = ParseOneStatement();
					if (whiteStatement != null)
					{
						if (CheckForNextPragmaStatment(whiteStatement))
						{
							next = (IWhitePragmaStatement)whiteStatement;
							return whiteSequenceStatement;
						}
						whiteSequenceStatement.Add(whiteStatement);
						if (!(whiteStatement is IWhiteFinalStatement))
						{
							continue;
						}
						return whiteSequenceStatement;
					}
					return whiteSequenceStatement;
				}
				catch (ResynchronizeException resynchronizeException)
				{
					whiteSequenceStatement.Add(ParseErrorUntilEndOfStatement(resynchronizeException));
				}
			}
		}

		private IWhiteExpression ParsePrefixOperator(IWhitePrefixedOperatorToken token)
		{
			if (!(token is ISizeOfToken) && !(token is IXSizeOfToken))
			{
				if (token is I__CastToken)
				{
					return ParseCastExpression(token);
				}
				return ParsePrefixedOperatorExpression(token);
			}
			return ParseSizeOfExpression((IAnySizeOfToken)token);
		}

		private IWhiteExpression ParseSpecialScopeExpression(IWhiteScopeToken scope)
		{
			IPeriodToken period = TokenControl.Next<IPeriodToken>();
			IWhiteExpression right = ParseSTOperand();
			return new WhiteSpecialScopeExpression(scope, period, right);
		}

		private IWhiteExpression ParseGlobalScopeExpression(IPeriodToken period)
		{
			IWhiteVariableExpression right = new WhiteVariableExpression(TokenControl.Next<IIdentifierToken>());
			return ParseVarAccess(new WhiteGlobalScopeExpression(period, right));
		}

		private IWhiteExpression ParsePrefixedOperatorExpression(IWhitePrefixedOperatorToken operatorToken)
		{
			IRightParenthesisToken rightParenthesis;
			IList<IWhiteExpression> operands;
			ILeftParenthesisToken leftParenthesis = ParseOperandList(out rightParenthesis, out operands);
			return new WhitePrefixedOperatorExpression(operatorToken, leftParenthesis, operands, rightParenthesis);
		}

		private ILeftParenthesisToken ParseOperandList(out IRightParenthesisToken rightParenthesis, out IList<IWhiteExpression> operands)
		{
			ILeftParenthesisToken result = TokenControl.Next<ILeftParenthesisToken>();
			operands = new List<IWhiteExpression>();
			IRightParenthesisToken token;
			while (!TokenControl.TryNext<IRightParenthesisToken>(out token))
			{
				TokenControl.TryNext<ICommaToken>(out var token2);
				IWhiteExpression whiteExpression = ParseAssignExp();
				if (token2 != null)
				{
					whiteExpression = new WhiteLeadByCommaExpression(token2, whiteExpression);
				}
				operands.Add(whiteExpression);
			}
			rightParenthesis = token;
			return result;
		}

		private IWhitePrefixedOperatorExpression ParseCastExpression(IWhitePrefixedOperatorToken token)
		{
			ILeftParenthesisToken leftParenthesis = TokenControl.Next<ILeftParenthesisToken>();
			IWhiteExpression whiteExpression = ParseAssignExp();
			ILeadByCommaExpression leadByCommaExpression = ParseLeadByComma(ParseTypeOrExpression);
			IRightParenthesisToken rightParenthesis = TokenControl.Next<IRightParenthesisToken>();
			return new WhitePrefixedOperatorExpression(token, leftParenthesis, new IWhiteExpression[2] { whiteExpression, leadByCommaExpression }, rightParenthesis);
		}

		private IWhitePrefixedOperatorExpression ParseSizeOfExpression(IAnySizeOfToken token)
		{
			ILeftParenthesisToken leftParenthesis = TokenControl.Next<ILeftParenthesisToken>();
			IWhiteExpression whiteExpression = ParseTypeOrExpression();
			IRightParenthesisToken rightParenthesis = TokenControl.Next<IRightParenthesisToken>();
			return new WhitePrefixedOperatorExpression(token, leftParenthesis, new IWhiteExpression[1] { whiteExpression }, rightParenthesis);
		}

		private IWhiteExpression ParseConversionExpression(IConversionToken conversionToken)
		{
			ILeftParenthesisToken leftParenthesis = TokenControl.Next<ILeftParenthesisToken>();
			IWhiteExpression expression = ParseAssignExp();
			IRightParenthesisToken rightParenthesis = TokenControl.Next<IRightParenthesisToken>();
			return new WhiteConversionExpression(conversionToken, leftParenthesis, expression, rightParenthesis);
		}

		private IWhiteExpression ParseUnaryOperatorExpression(IWhiteOperatorToken unaryOperator)
		{
			IWhiteExpression operand = ParseSTOperand();
			return new WhiteUnaryOperatorExpression(unaryOperator, operand);
		}

		private IWhiteArrayInitializationExpression ParseArrayInitialization(ILeftBracketToken leftBracket)
		{
			IList<IWhiteExpression> list = new List<IWhiteExpression>();
			IRightBracketToken token;
			while (!TokenControl.TryNext<IRightBracketToken>(out token))
			{
				if (TokenControl.TryNext<ICommaToken>(out var token2) && TokenControl.TryNext<IRightBracketToken>(out token))
				{
					list.Add(new WhiteLeadByCommaExpression(token2, new WhiteEmptyExpression()));
					break;
				}
				IWhiteExpression whiteExpression = ParseAssignExp();
				if (TokenControl.CheckNext<ILeftParenthesisToken>())
				{
					IWhiteExpression value = ParseAssignExp();
					whiteExpression = new WhiteMultipleIndexInitialization(whiteExpression, value);
				}
				if (token2 != null)
				{
					whiteExpression = new WhiteLeadByCommaExpression(token2, whiteExpression);
				}
				list.Add(whiteExpression);
			}
			return new WhiteArrayInitializationExpression(leftBracket, list, token);
		}

		private IWhiteExpression ParseTypeOrExpression()
		{
			int currentTokenIndex = TokenControl.CurrentTokenIndex;
			try
			{
				IWhiteTypeExpression whiteTypeExpression = new TypeExpressionParser(TokenControl).ParseType();
				if (whiteTypeExpression.Class != TypeClass.Userdef)
				{
					return whiteTypeExpression;
				}
			}
			catch
			{
			}
			TokenControl.CurrentTokenIndex = currentTokenIndex;
			return ParseAssignExp();
		}

		private ILeadByCommaExpression ParseLeadByComma(Func<IWhiteExpression> expressionProvider)
		{
			ICommaToken comma = TokenControl.Next<ICommaToken>();
			IWhiteExpression expression = expressionProvider();
			return new WhiteLeadByCommaExpression(comma, expression);
		}

		private IWhiteStatement ParseRepeatStatement()
		{
			IRepeatToken repeat = TokenControl.Next<IRepeatToken>();
			IWhiteSequenceStatement controlled = ParseST();
			IUntilToken until = TokenControl.Next<IUntilToken>();
			IWhiteExpression condition = ParseAssignExp();
			IEndRepeatToken endRepeat = TokenControl.Next<IEndRepeatToken>();
			return new WhiteRepeatStatement(repeat, controlled, until, condition, endRepeat);
		}

		private IWhiteDocuCommentStatement ParseDocuCommentStatement()
		{
			IDocCommentToken token = (IDocCommentToken)TokenControl.Next(statementStart: true);
			DisconnectFromTrailing(token);
			return new WhiteDocuCommentStatement(token);
		}

		private IWhiteCommentStatement ParseCommentStatement()
		{
			ICommentToken token = (ICommentToken)TokenControl.Next(statementStart: true);
			DisconnectFromTrailing(token);
			return new WhiteCommentStatement(token);
		}

		private IWhiteLabelStatement ParseLabelStatement()
		{
			IIdentifierToken labelToken = TokenControl.Next<IIdentifierToken>();
			IColonToken colonToken = TokenControl.Next<IColonToken>();
			return new WhiteLabelStatement(labelToken, colonToken);
		}

		private IWhiteStatement ParseIdentifierStatement()
		{
			int currentTokenIndex = TokenControl.CurrentTokenIndex;
			TokenControl.LookAhead2(out var _, out var second);
			if (second is IColonToken)
			{
				return ParseLabelStatement();
			}
			IWhiteExpression whiteExpression = ParseAssignExp();
			int currentTokenIndex2 = TokenControl.CurrentTokenIndex;
			try
			{
				ISemicolonToken semicolon = TokenControl.Next<ISemicolonToken>();
				return new WhiteExpressionStatement(whiteExpression, semicolon);
			}
			catch (ResynchronizeException resynchronizeException)
			{
				return ParseErrorFromTokenRange(currentTokenIndex, currentTokenIndex2, useStatementStart: true, resynchronizeException);
			}
		}

		private IWhiteStatement ParseTryCatchStatement()
		{
			ITryToken @try = TokenControl.Next<ITryToken>();
			IWhiteSequenceStatement trySequence = ParseST();
			ICatchToken @catch = TokenControl.Next<ICatchToken>();
			IParenthesizedExpression exceptionExpression = TryParseParenthesizedExpression() as IParenthesizedExpression;
			IWhiteSequenceStatement catchSequence = ParseST();
			IWhiteSequenceStatement finallySequence = null;
			if (TokenControl.TryNext<IFinallyToken>(out var token))
			{
				finallySequence = ParseST();
			}
			return new WhiteTryCatchStatement(endtry: TokenControl.Next<IEndTryToken>(), @try: @try, trySequence: trySequence, @catch: @catch, exceptionExpression: exceptionExpression, catchSequence: catchSequence, @finally: token, finallySequence: finallySequence);
		}

		private IWhiteStatement ParseVariableDeclaration()
		{
			IList<IWhiteExpression> list = new List<IWhiteExpression>();
			IIdentifierToken token = TokenControl.Next<IIdentifierToken>();
			list.Add(new WhiteVariableExpression(token));
			ICommaToken token2;
			while (TokenControl.TryNext<ICommaToken>(out token2))
			{
				WhiteVariableExpression expression = new WhiteVariableExpression(TokenControl.Next<IIdentifierToken>());
				WhiteLeadByCommaExpression item = new WhiteLeadByCommaExpression(token2, expression);
				list.Add(item);
			}
			IWhiteAnyDirectVariableExpression addressLocation = null;
			if (TokenControl.TryNext<IAtToken>(out var token3))
			{
				addressLocation = ((!TokenControl.TryNext<IDirectVariableToken>(out var token4)) ? ((IWhiteAnyDirectVariableExpression)new WhiteIncompleteDirectVariableExpression(TokenControl.Next<IIncompleteDirectVariableToken>())) : ((IWhiteAnyDirectVariableExpression)new WhiteDirectVariableExpression(token4)));
			}
			IColonToken colon = TokenControl.Next<IColonToken>();
			IWhiteTypeExpression declaredType = new TypeExpressionParser(TokenControl).ParseType();
			IWhiteExpression initializationExpression = null;
			if (TokenControl.TryNext<IAnyAssignmentToken>(out var token5))
			{
				initializationExpression = ParseAssignExp();
			}
			ISemicolonToken semicolon = TokenControl.Next<ISemicolonToken>();
			return new WhiteVariableDeclarationStatement(list, token3, addressLocation, colon, declaredType, token5, initializationExpression, semicolon);
		}

		private IWhiteStatement ParseVariableList()
		{
			int startStatementTokenIndex = TokenControl.StartStatementTokenIndex;
			IVarListStartToken varListStartToken = TokenControl.Next<IVarListStartToken>();
			List<IWhiteOperatorToken> persistantRetain = ParseVariableDeclarationListFlags(varListStartToken);
			int num = 0;
			WhiteSequenceStatement whiteSequenceStatement = new WhiteSequenceStatement();
			while (num++ < 1000000)
			{
				IWhiteToken whiteToken = TokenControl.LookAhead1(bStatementStart: true);
				if (whiteToken is IVarListEndToken || whiteToken is EndToken)
				{
					break;
				}
				int startStatementTokenIndex2 = TokenControl.StartStatementTokenIndex;
				try
				{
					TokenControl.StartStatementTokenIndex = TokenControl.CurrentTokenIndex;
					IWhiteStatement whiteStatement = ((whiteToken is IIdentifierToken) ? ParseVariableDeclaration() : ParseOneStatement());
					if (whiteStatement != null)
					{
						whiteSequenceStatement.Add(whiteStatement);
					}
				}
				catch (ResynchronizeException resynchronizeException)
				{
					whiteSequenceStatement.Add(ParseErrorUntilEndOfStatement(resynchronizeException));
				}
				finally
				{
					TokenControl.StartStatementTokenIndex = startStatementTokenIndex2;
				}
			}
			IWhiteToken whiteToken2 = TokenControl.Next();
			if (whiteToken2 is IVarListEndToken endVarOp)
			{
				return new WhiteVariableDeclarationListStatement(varListStartToken, persistantRetain, whiteSequenceStatement, endVarOp);
			}
			TokenControl.StartStatementTokenIndex = startStatementTokenIndex;
			string errorMessage = WhiteParserMessages.UnexpectedToken(whiteToken2, typeof(IVarListEndToken));
			return ParseErrorUntil(TokenControl.StartStatementTokenIndex, useStatementStart: true, IsStartOfDeclarationBlockToken, errorMessage, whiteToken2);
		}

		private List<IWhiteOperatorToken> ParseVariableDeclarationListFlags(IVarListStartToken startToken)
		{
			List<IWhiteOperatorToken> list = new List<IWhiteOperatorToken>();
			while (true)
			{
				if (TokenControl.TryNext<IVarTypePrefixToken>(out var token))
				{
					list.Add(token);
					continue;
				}
				if (!(startToken is IVarGlobalToken) || !TokenControl.TryNext<IInternalToken>(out var token2))
				{
					break;
				}
				list.Add(token2);
			}
			return list;
		}

		private IWhiteStatement ParseWhileStatement()
		{
			IWhileToken @while = TokenControl.Next<IWhileToken>();
			IWhiteExpression condition = ParseAssignExp();
			IDoToken @do = TokenControl.Next<IDoToken>();
			IWhiteSequenceStatement controlled = ParseST();
			IEndWhileToken endWhile = TokenControl.Next<IEndWhileToken>();
			return new WhiteWhileStatement(@while, condition, @do, controlled, endWhile);
		}
	}
}
