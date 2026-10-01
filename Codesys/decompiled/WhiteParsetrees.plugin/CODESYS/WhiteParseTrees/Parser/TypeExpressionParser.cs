using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Nodes;
using CODESYS.WhiteParseTrees.Nodes.Expressions;

namespace CODESYS.WhiteParseTrees.Parser
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class TypeExpressionParser
	{
		private TokenControl TokenControl { get; }

		public TypeExpressionParser(TokenControl tokenControl)
		{
			TokenControl = tokenControl;
		}

		public IWhiteTypeExpression ParseType()
		{
			TokenControl.LookAhead2(out var first, out var _);
			if (first is IWhiteSimpleTypeToken)
			{
				IWhiteSimpleTypeToken whiteSimpleTypeToken = TokenControl.Next<IWhiteSimpleTypeToken>();
				WhiteSimpleTypeExpression whiteSimpleTypeExpression = new WhiteSimpleTypeExpression(whiteSimpleTypeToken);
				if (IsSubRangeType(whiteSimpleTypeToken) && TokenControl.TryNext<ILeftParenthesisToken>(out var token))
				{
					WhiteTreeParser whiteTreeParser = new WhiteTreeParser(TokenControl);
					IWhiteExpression low = whiteTreeParser.ParseAssignExp();
					IRangeToken range = TokenControl.Next<IRangeToken>();
					IWhiteExpression high = whiteTreeParser.ParseAssignExp();
					IRightParenthesisToken rightParenthesis = TokenControl.Next<IRightParenthesisToken>();
					return new SubRangeTypeExpression(whiteSimpleTypeExpression, token, new WhiteRangeExpression(low, range, high), rightParenthesis);
				}
				return whiteSimpleTypeExpression;
			}
			switch (first.Type)
			{
			case WhiteTokenType.Pointer:
				return ParsePointerType();
			case WhiteTokenType.Reference:
				return ParseReferenceType();
			case WhiteTokenType.String:
			case WhiteTokenType.WString:
			case WhiteTokenType.__XString:
				return ParseStringType();
			case WhiteTokenType.__Vector:
				return ParseVectorType();
			case WhiteTokenType.Array:
				return ParseArrayType();
			case WhiteTokenType.Identifier:
			case WhiteTokenType.This:
			case WhiteTokenType.Super:
			case WhiteTokenType.__SystemScope:
			case WhiteTokenType.__PoolScope:
				return new WhiteUserDefTypeExpression(new WhiteTreeParser(TokenControl).ParseSTOperand());
			case WhiteTokenType.LeftParenthesis:
				return ParseEnumerationType();
			default:
			{
				string message = WhiteParserMessages.UnexpectedToken(first, "Type");
				throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, first);
			}
			}
		}

		private IWhiteTypeExpression ParseEnumerationType()
		{
			ILeftParenthesisToken leftParenthesis = TokenControl.Next<ILeftParenthesisToken>();
			List<IWhiteExpression> list = new List<IWhiteExpression>();
			while (true)
			{
				IWhiteToken whiteToken = TokenControl.LookAhead1();
				if (whiteToken is IRightParenthesisToken)
				{
					break;
				}
				if (whiteToken is ICommaToken)
				{
					ICommaToken comma = TokenControl.Next<ICommaToken>();
					IWhiteExpression expression = ParseType();
					list.Add(new WhiteLeadByCommaExpression(comma, expression));
				}
				else
				{
					list.Add(ParseType());
				}
			}
			IRightParenthesisToken rightParenthesis = TokenControl.Next<IRightParenthesisToken>();
			return new EnumerationTypeExpression(leftParenthesis, list, rightParenthesis);
		}

		private IWhiteExpression ParseRangeExpression()
		{
			WhiteTreeParser whiteTreeParser = new WhiteTreeParser(TokenControl);
			IWhiteExpression low = whiteTreeParser.ParseAssignExp();
			IRangeToken range = TokenControl.Next<IRangeToken>();
			IWhiteExpression high = whiteTreeParser.ParseAssignExp();
			return new WhiteRangeExpression(low, range, high);
		}

		private IWhiteTypeExpression ParseArrayType()
		{
			IArrayToken array = TokenControl.Next<IArrayToken>();
			ILeftBracketToken leftBracket = TokenControl.Next<ILeftBracketToken>();
			IWhiteToken whiteToken = TokenControl.LookAhead1();
			if (whiteToken is ITimesToken)
			{
				ITimesToken timesOp = TokenControl.Next<ITimesToken>();
				IRightBracketToken rightBracket = TokenControl.Next<IRightBracketToken>();
				IOfToken of = TokenControl.Next<IOfToken>();
				IWhiteTypeExpression baseType = ParseType();
				return new WhiteVariableArrayTypeExpression(array, leftBracket, timesOp, rightBracket, of, baseType);
			}
			List<IWhiteExpression> list = new List<IWhiteExpression> { ParseRangeExpression() };
			while (true)
			{
				whiteToken = TokenControl.LookAhead1();
				if (whiteToken is IRightBracketToken)
				{
					break;
				}
				if (whiteToken is ICommaToken)
				{
					ICommaToken comma = TokenControl.Next<ICommaToken>();
					IWhiteExpression expression = ParseRangeExpression();
					list.Add(new WhiteLeadByCommaExpression(comma, expression));
					continue;
				}
				string message = WhiteParserMessages.UnexpectedToken(whiteToken, typeof(IRightBracketToken));
				throw new ResynchronizeException(TokenControl.StartStatementTokenIndex, message, whiteToken);
			}
			IRightBracketToken rightBracket2 = TokenControl.Next<IRightBracketToken>();
			IOfToken of2 = TokenControl.Next<IOfToken>();
			IWhiteTypeExpression baseType2 = ParseType();
			return new WhiteArrayTypeExpression(array, leftBracket, list, rightBracket2, of2, baseType2);
		}

		private IWhiteTypeExpression ParseVectorType()
		{
			I__VectorToken vector = TokenControl.Next<I__VectorToken>();
			ILeftBracketToken leftBracket = TokenControl.Next<ILeftBracketToken>();
			IWhiteExpression length = new WhiteTreeParser(TokenControl).ParseSTOperand();
			IRightBracketToken rightBracket = TokenControl.Next<IRightBracketToken>();
			IOfToken of = TokenControl.Next<IOfToken>();
			IWhiteTypeExpression baseType = ParseType();
			return new VectorTypeExpression(vector, leftBracket, length, rightBracket, of, baseType);
		}

		private IWhiteTypeExpression ParsePointerType()
		{
			IPointerToken pointer = TokenControl.Next<IPointerToken>();
			IToToken to = TokenControl.Next<IToToken>();
			IWhiteTypeExpression baseType = ParseType();
			return new WhitePointerTypeExpression(pointer, to, baseType);
		}

		private IWhiteTypeExpression ParseReferenceType()
		{
			IReferenceToken reference = TokenControl.Next<IReferenceToken>();
			IToToken to = TokenControl.Next<IToToken>();
			IWhiteTypeExpression baseType = ParseType();
			return new WhiteReferenceTypeExpression(reference, to, baseType);
		}

		private IWhiteTypeExpression ParseStringType()
		{
			IAnyStringSimpleTypeToken stringSimpleOperator = TokenControl.Next<IAnyStringSimpleTypeToken>();
			if (TokenControl.TryNext<IAnyBraceLeftToken>(out var token))
			{
				IWhiteExpression length = new WhiteTreeParser(TokenControl).ParseAssignExp();
				IAnyBraceRightToken rightParenthesis = TokenControl.Next<IAnyBraceRightToken>();
				return new StringTypeExpression(stringSimpleOperator, token, length, rightParenthesis);
			}
			return new StringTypeExpression(stringSimpleOperator, null, null, null);
		}

		private static bool IsSubRangeType(IWhiteSimpleTypeToken typeToken)
		{
			return TypeTable.IsInteger(TypeTable.GetTypeByOperator(typeToken.Operator));
		}
	}
}
