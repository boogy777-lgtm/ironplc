using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Resources;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal class OperandParser
	{
		private ParserContext Context { get; }

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		internal OperandParser(ParserContext context)
		{
			Context = context;
		}

		internal IExpression ParseOperand()
		{
			bool bError;
			_IExpression result = ParseSTOperand(out bError);
			if (bError)
			{
				return null;
			}
			return result;
		}

		internal _IExpression ParseSTOperand(out bool bError)
		{
			_IToken endToken;
			return ParseSTOperandWithPosition(out bError, null, out endToken);
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private _IExpression ParseArrayAccess(_IExpression exp, out bool bError, _IToken currentToken, ref _IToken endToken)
		{
			_IIndexAccessExpression iIndexAccessExpression = LMItemFactory.CreateIndexAccessExpression(exp, currentToken);
			_IExpression iExpression = ExpressionParser.ParseAssignment(out bError);
			if (iExpression == null)
			{
				return null;
			}
			iIndexAccessExpression.AddAccess(iExpression);
			IToken token;
			while (true)
			{
				if (Next(out token) != TokenType.Operator)
				{
					ErrorHandler.AddErrorSTWithToken(iIndexAccessExpression, token, MessageId.Err_Operator1of2Expected, Scanner.GetOperatorText(Operator.Comma), Scanner.GetOperatorText(Operator.RightBracket), Scanner.GetTokenText(token));
					bError = true;
					return iIndexAccessExpression;
				}
				Operator @operator = Scanner.GetOperator(token);
				if (@operator != Operator.Comma && @operator != Operator.RightBracket)
				{
					ErrorHandler.AddErrorSTWithToken(iIndexAccessExpression, token, MessageId.Err_Operator1of2Expected, Scanner.GetOperatorText(Operator.Comma), Scanner.GetOperatorText(Operator.RightBracket), Scanner.GetTokenText(token));
					bError = true;
					return iIndexAccessExpression;
				}
				if (@operator != Operator.Comma)
				{
					break;
				}
				iExpression = ExpressionParser.ParseAssignment(out bError);
				iIndexAccessExpression.AddAccess(iExpression);
			}
			endToken = token as _IToken;
			return iIndexAccessExpression;
		}

		private _IExpression ParseNamespaceAccess(_IExpression exp, out bool bError, _IToken currentToken, out _IToken endToken)
		{
			_INamespaceAccessExpression iNamespaceAccessExpression = LMItemFactory.CreateNamespaceAccessExpression(exp, null, currentToken);
			bError = false;
			if (Next(out var token) == TokenType.Identifier)
			{
				string identifier = Scanner.GetIdentifier(token);
				_IVariableExpression iVariableExpression = (_IVariableExpression)(iNamespaceAccessExpression._Access = LMItemFactory.CreateVariableExpression(identifier, token));
			}
			else
			{
				iNamespaceAccessExpression._Access = LMItemFactory.CreateNullExpression(token);
				ErrorHandler.AddErrorSTWithToken(iNamespaceAccessExpression, token, MessageId.Err_NoComponentOf, Scanner.GetTokenText(token), exp);
				bError = true;
			}
			endToken = token as _IToken;
			return iNamespaceAccessExpression;
		}

		private _IExpression ParseComponentAccess(_IExpression exp, out bool bError, _IToken currentToken, out _IToken endToken)
		{
			_ICompoAccessExpression iCompoAccessExpression = LMItemFactory.CreateCompoAccessExpression(exp, currentToken);
			bError = false;
			IToken token;
			switch (Next(out token))
			{
			case TokenType.Integer:
			{
				Scanner.GetInteger(token, out var nValue, out var bSign, out var _, out var bOverflow);
				if (nValue >= 64 || bSign || bOverflow)
				{
					ErrorHandler.AddErrorSTWithToken(iCompoAccessExpression, token, MessageId.Err_BitNrOverflow, nValue, exp);
				}
				iCompoAccessExpression._Right = LMItemFactory.CreateLiteralExpression((long)nValue, TypeClass.AnyInt, token);
				endToken = token as _IToken;
				return iCompoAccessExpression;
			}
			case TokenType.PartialAccess:
			{
				((_IScanner4)Scanner).GetPartialAccess(token, out var partSize, out var partOffset, out var overflow);
				_IPartialAccessExpression iPartialAccessExpression = LMItemFactory.CreatePartialAccessExpression(token, exp, partSize, partOffset);
				if (overflow)
				{
					ErrorHandler.AddErrorST(iPartialAccessExpression, MessageId.Err_BitNrOverflow, int.MaxValue, exp);
				}
				endToken = token as _IToken;
				if (Context._bReportSP19Feature)
				{
					Context.AddUnsupportedFeatureError(iPartialAccessExpression, Strings.CompilerFeature_PartialVariableAccess, ParserContext.CompilerVersion19);
				}
				return iPartialAccessExpression;
			}
			case TokenType.Identifier:
			{
				string identifier = Scanner.GetIdentifier(token);
				_IVariableExpression iVariableExpression = (_IVariableExpression)(iCompoAccessExpression._Right = LMItemFactory.CreateVariableExpression(identifier, token));
				break;
			}
			default:
				ErrorHandler.AddErrorSTWithToken(iCompoAccessExpression, token, MessageId.Err_NoComponentOf, Scanner.GetTokenText(token), exp);
				bError = true;
				break;
			}
			endToken = token as _IToken;
			return iCompoAccessExpression;
		}

		internal _IExpression ParseVarAccess(_IExpression exp, out bool bError, _IToken startToken, ref _IToken endToken)
		{
			bError = false;
			if (Next(out var token) != TokenType.Operator)
			{
				Scanner.SetPosition(token);
				return exp;
			}
			_IExpression iExpression;
			switch (Scanner.GetOperator(token))
			{
			case Operator.DeRef:
				iExpression = LMItemFactory.CreateDeRefAccessExpression(exp, token);
				endToken = token as _IToken;
				break;
			case Operator.LeftBracket:
				iExpression = ParseArrayAccess(exp, out bError, token as _IToken, ref endToken);
				break;
			case Operator.Period:
				iExpression = ParseComponentAccess(exp, out bError, token as _IToken, out endToken);
				break;
			case Operator.Hash:
				iExpression = ParseNamespaceAccess(exp, out bError, token as _IToken, out endToken);
				break;
			case Operator.LeftParenthesis:
				iExpression = FunctionCallParser.ParseFunctionCall(Context, exp, out bError, token as _IToken, ref endToken);
				break;
			default:
				Scanner.SetPosition(token);
				return exp;
			}
			if (endToken == null)
			{
				endToken = startToken;
			}
			iExpression.PositionLength = Helper.CalculateLength(startToken, endToken);
			return ParseVarAccess(iExpression, out bError, startToken, ref endToken);
		}

		internal _IExpression ParseSTOperandWithPosition(out bool bError, _IToken startToken, out _IToken endToken)
		{
			if (startToken == null)
			{
				Next(out var token);
				Scanner.SetPosition(token);
				startToken = token as _IToken;
			}
			short lenghtOfExpWithoutParanthesis;
			_IExpression iExpression = ParseSTOperandHelp(out bError, startToken, out endToken, out lenghtOfExpWithoutParanthesis);
			if (endToken == null)
			{
				endToken = (_IToken)Scanner.CurrentToken;
			}
			iExpression.PositionLength = ((lenghtOfExpWithoutParanthesis != 0) ? lenghtOfExpWithoutParanthesis : Helper.CalculateLength(startToken, endToken));
			return iExpression;
		}

		internal _IExpression ParseQualifiedNameExpression(_IExprement expForError)
		{
			int sourceOffset = Scanner.SourceOffset;
			_IExpression iExpression = null;
			bool bSystemScope;
			bool bPoolScope;
			IToken token = CheckForSpecialScopes(out bSystemScope, out bPoolScope);
			Operator @operator = Operator.None;
			IToken token3;
			do
			{
				Next(out var token2);
				if (token2.Type != TokenType.Identifier)
				{
					if (expForError != null)
					{
						ErrorHandler.AddErrorSTWithToken(expForError, token2, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(token2));
					}
					Scanner.ParseReSyncIF();
					return null;
				}
				if (iExpression == null)
				{
					iExpression = LMItemFactory.CreateVariableExpression(Scanner.GetIdentifier(token2), token2);
				}
				else
				{
					switch (@operator)
					{
					case Operator.Period:
					{
						_ICompoAccessExpression iCompoAccessExpression = LMItemFactory.CreateCompoAccessExpression(iExpression, token2);
						iCompoAccessExpression._Right = LMItemFactory.CreateVariableExpression(Scanner.GetIdentifier(token2), token2);
						iExpression = iCompoAccessExpression;
						break;
					}
					case Operator.Hash:
					{
						_INamespaceAccessExpression iNamespaceAccessExpression = LMItemFactory.CreateNamespaceAccessExpression(iExpression, null, token2);
						iNamespaceAccessExpression._Access = LMItemFactory.CreateVariableExpression(Scanner.GetIdentifier(token2), token2);
						iExpression = iNamespaceAccessExpression;
						break;
					}
					}
				}
				Scanner.Next(out token3, bWithPragma: false, bWithComment: true);
				Scanner.SetPosition(token3);
				@operator = Scanner.MatchOperator(ErrorHandler, null, false, Operator.Period, Operator.Hash);
			}
			while (@operator != 0);
			Scanner.SetPosition(token3);
			if (iExpression == null)
			{
				return null;
			}
			if (bSystemScope)
			{
				iExpression = LMItemFactory.CreateSystemScopeExpression(iExpression, token);
			}
			else if (bPoolScope)
			{
				iExpression = LMItemFactory.CreatePoolScopeExpression(iExpression, token);
			}
			int sourceOffset2 = Scanner.SourceOffset;
			iExpression.PositionLength = (short)(sourceOffset2 - sourceOffset);
			return iExpression;
		}

		private IToken CheckForSpecialScopes(out bool bSystemScope, out bool bPoolScope)
		{
			bSystemScope = false;
			bPoolScope = false;
			Next(out var token);
			if (token.Type == TokenType.Operator && Scanner.GetOperator(token) == Operator.__SystemScope && Next(out var token2) == TokenType.Operator && Scanner.GetOperator(token2) == Operator.Period)
			{
				bSystemScope = true;
			}
			else if (token.Type == TokenType.Operator && Scanner.GetOperator(token) == Operator.__PoolScope && Next(out token2) == TokenType.Operator && Scanner.GetOperator(token2) == Operator.Period)
			{
				bPoolScope = true;
			}
			else
			{
				Scanner.SetPosition(token);
			}
			return token;
		}

		private _IExpression ParseSTOperandHelp(out bool bError, _IToken startToken, out _IToken endToken, out short lenghtOfExpWithoutParanthesis)
		{
			bError = false;
			Next(out var token);
			_IExpression iExpression = null;
			lenghtOfExpWithoutParanthesis = 0;
			endToken = token as _IToken;
			switch (token.Type)
			{
			case TokenType.Boolean:
			case TokenType.Date:
			case TokenType.DateAndTime:
			case TokenType.DoubleByteString:
			case TokenType.Duration:
			case TokenType.LDuration:
			case TokenType.Integer:
			case TokenType.Real:
			case TokenType.SingleByteString:
			case TokenType.TimeOfDay:
			case TokenType.XByteString:
			case TokenType.LDate:
			case TokenType.LTimeOfDay:
			case TokenType.LDateAndTime:
				iExpression = LMItemFactory.CreateLiteralExpression(Context, token);
				endToken = token as _IToken;
				break;
			case TokenType.Identifier:
			{
				string identifier = Scanner.GetIdentifier(token);
				_IVariableExpression exp = LMItemFactory.CreateVariableExpression(identifier, token);
				iExpression = ParseVarAccess(exp, out bError, token as _IToken, ref endToken);
				break;
			}
			case TokenType.DirectVariable:
			{
				Scanner.GetDirectVariable(token, out var location, out var size, out var components, out var bOverflow);
				iExpression = LMItemFactory.CreateAddressExpression(LMItemFactory.CreateDirectVariable(location, size, components), token);
				if (bOverflow)
				{
					ErrorHandler.AddErrorST(iExpression, MessageId.Err_OverflowInAddress, iExpression);
				}
				endToken = token as _IToken;
				break;
			}
			case TokenType.Operator:
			{
				Operator @operator = Scanner.GetOperator(token);
				iExpression = ExpressionParser.ParseSTPrefixOperator(out bError, @operator, token as _IToken, startToken, out endToken, out lenghtOfExpWithoutParanthesis);
				break;
			}
			}
			if (iExpression == null)
			{
				iExpression = LMItemFactory.CreateErrorExpression(token);
				ErrorHandler.AddErrorST(iExpression, MessageId.Err_ExpressionExpectedInstead, Scanner.GetTokenText(token));
				Scanner.SetPosition(token);
				bError = true;
			}
			return iExpression;
		}
	}
}
