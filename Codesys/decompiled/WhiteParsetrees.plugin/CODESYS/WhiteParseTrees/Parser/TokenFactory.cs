using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.WhiteParseTrees.Nodes;
using CODESYS.WhiteParseTrees.Nodes.Factories;
using CODESYS.WhiteParseTrees.Nodes.Tokens;
using CODESYS.WhiteParseTrees.Services;

namespace CODESYS.WhiteParseTrees.Parser
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class TokenFactory
	{
		private static void TryRecognizeContextualMethodOperators(IScanner7 scanner, bool inMethodDecl)
		{
			if (scanner is _IScanner6 iScanner)
			{
				iScanner.RecognizeContextualOperator(inMethodDecl, Operator.Overload);
				iScanner.RecognizeContextualOperator(inMethodDecl, Operator.Override);
			}
		}

		public static IWhiteToken CreateToken(IToken token, IScanner7 scanner)
		{
			switch (token.Type)
			{
			case TokenType.Boolean:
				return new BooleanToken(scanner.GetTokenText(token), scanner.GetBoolean(token));
			case TokenType.Comment:
				return new CommentToken(scanner.GetTokenText(token), scanner.GetComment(token));
			case TokenType.DocComment:
				return new DocCommentToken(scanner.GetTokenText(token), scanner.GetDocComment(token));
			case TokenType.Pragma:
				return new WhitePragmaToken(scanner.GetTokenText(token), scanner.GetPragma(token));
			case TokenType.Date:
			{
				scanner.GetDate(token, out var value6, out var bOverflow11);
				return new DateToken(scanner.GetTokenText(token), value6, bOverflow11);
			}
			case TokenType.DateAndTime:
			{
				scanner.GetDateAndTime(token, out var value5, out var bOverflow10);
				return new DateAndTimeToken(scanner.GetTokenText(token), value5, bOverflow10);
			}
			case TokenType.LDate:
			{
				scanner.GetLDate(token, out var value4, out var bOverflow9);
				return new LDateToken(scanner.GetTokenText(token), value4, bOverflow9);
			}
			case TokenType.DirectVariable:
			{
				scanner.GetDirectVariable(token, out var location2, out var size, out var components, out var bOverflow8);
				return new DirectVariableToken(scanner.GetTokenText(token), location2, size, components, bOverflow8);
			}
			case TokenType.IncompleteDirectVariable:
			{
				scanner.GetIncompleteDirectVariable(token, out var location);
				return new IncompleteDirectVariableToken(scanner.GetTokenText(token), location);
			}
			case TokenType.DoubleByteString:
				return new DoubleByteStringToken(scanner.GetTokenText(token), scanner.GetDoubleByteString(token));
			case TokenType.Duration:
			{
				scanner.GetDuration(token, out var nDuration, out var bOverflow7);
				return new DurationToken(scanner.GetTokenText(token), nDuration, bOverflow7);
			}
			case TokenType.LDuration:
			{
				scanner.GetLDuration(token, out var ulDuration, out var bOverflow6);
				return new LDurationToken(scanner.GetTokenText(token), ulDuration, bOverflow6);
			}
			case TokenType.EndOfLine:
				return new EndOfLineToken(scanner.GetTokenText(token));
			case TokenType.Identifier:
				return new IdentifierToken(scanner.GetTokenText(token));
			case TokenType.Integer:
			{
				scanner.GetInteger(token, out var nValue, out var _, out var _, out var _);
				return new IntegerToken(scanner.GetTokenText(token), nValue);
			}
			case TokenType.Operator:
			{
				Operator @operator = scanner.GetOperator(token);
				switch (@operator)
				{
				case Operator.Conversion:
				{
					scanner.GetConversion(token, out var sourceType, out var destType);
					return new ConversionOperatorToken(scanner.GetTokenText(token), @operator, TypeTable.GetTypeByOperator(sourceType), TypeTable.GetTypeByOperator(destType));
				}
				case Operator.Method:
					TryRecognizeContextualMethodOperators(scanner, inMethodDecl: true);
					break;
				case Operator.EndMethod:
					TryRecognizeContextualMethodOperators(scanner, inMethodDecl: false);
					break;
				}
				return OperatorTokenFactory.CreateOperatorTokenByOperator(scanner.GetTokenText(token), scanner.GetOperator(token));
			}
			case TokenType.Real:
			{
				scanner.GetReal(token, out var dValue, out var type, out var bOverflow4);
				return new RealToken(scanner.GetTokenText(token), dValue, type, bOverflow4);
			}
			case TokenType.SingleByteString:
				return new SingleByteStringToken(scanner.GetTokenText(token), scanner.GetSingleByteString(token));
			case TokenType.TimeOfDay:
			{
				scanner.GetTimeOfDay(token, out var value3, out var bOverflow3);
				return new TimeOfDayToken(scanner.GetTokenText(token), value3, bOverflow3);
			}
			case TokenType.Whitespace:
				return new WhitespaceToken(scanner.GetTokenText(token));
			case TokenType.Error:
				return new ErrorToken(scanner.GetTokenText(token));
			case TokenType.End:
				return new EndToken(string.Empty);
			case TokenType.XByteString:
				return new XByteStringToken(scanner.GetTokenText(token), scanner.GetXByteString(token));
			case TokenType.LTimeOfDay:
			{
				scanner.GetLTimeOfDay(token, out var value2, out var bOverflow2);
				return new LTimeOfDayToken(scanner.GetTokenText(token), value2, bOverflow2);
			}
			case TokenType.LDateAndTime:
			{
				scanner.GetLDateAndTime(token, out var value, out var bOverflow);
				return new LDateAndTimeToken(scanner.GetTokenText(token), value, bOverflow);
			}
			case TokenType.PartialAccess:
				if (scanner is _IScanner4 iScanner)
				{
					iScanner.GetPartialAccess(token, out var partSize, out var partOffset, out var overflow);
					return new PartialAccessToken(scanner.GetTokenText(token), partSize, partOffset, overflow);
				}
				throw new TooOldCompilerversionException(new Version(3, 5, 19, 0));
			default:
				throw new ArgumentOutOfRangeException("token");
			}
		}
	}
}
