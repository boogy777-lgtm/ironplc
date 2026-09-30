using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Expressions;
using CODESYS.WhiteParseTrees.Nodes.Tokens;
using CODESYS.WhiteParseTrees.Parser;

namespace CODESYS.WhiteParseTrees.Nodes.Factories
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class ExpressionFactory : IWhiteParseTreeExpressionFactory2, IWhiteParseTreeExpressionFactory
	{
		public T ParseExpression<[System.Runtime.CompilerServices.Nullable(0)] T>(string stInput) where T : IWhiteExpression
		{
			return (T)(ParseExpression(stInput) ?? throw new InvalidOperationException("stInput does not parse to expected type"));
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExpression ParseExpression(string stInput)
		{
			try
			{
				return WhiteTreeParser.ParseExpression(stInput);
			}
			catch (ResynchronizeException)
			{
				return null;
			}
		}

		public IWhiteAssignmentExpression CreateWhiteAssignmentExpression(IWhiteExpression expLValue, IWhiteExpression expRValue, IAnyAssignmentToken assignment)
		{
			return new WhiteAssignmentExpression(expLValue, expRValue, assignment);
		}

		public IWhiteBinaryOperatorExpression CreateWhiteBinaryOperatorExpression(IWhiteExpression first, IWhiteOperatorToken token, IWhiteExpression second)
		{
			return new WhiteBinaryOperatorExpression(first, token, second);
		}

		public IWhiteUnaryOperatorExpression CreateWhiteUnaryOperatorExpression(IWhiteOperatorToken token, IWhiteExpression operand)
		{
			return new WhiteUnaryOperatorExpression(token, operand);
		}

		public IWhiteVariableExpression CreateWhiteVariableExpression(string value)
		{
			return CreateWhiteVariableExpression(TokenFactory<IdentifierToken>.Create(value));
		}

		public IWhiteVariableExpression CreateWhiteVariableExpression(IIdentifierToken token)
		{
			return new WhiteVariableExpression(token);
		}

		public IWhiteIntegerLiteralExpression CreateWhiteIntegerLiteralExpression(IIntegerToken intToken)
		{
			return new WhiteIntegerLiteralExpression(intToken);
		}

		public IWhiteIntegerLiteralExpression CreateWhiteIntegerLiteralExpression(int value)
		{
			return new WhiteIntegerLiteralExpression(TokenFactory<IntegerToken>.Create(value.ToString()));
		}

		public IWhiteRealLiteralExpression CreateWhiteRealLiteralExpression(IRealToken realToken)
		{
			return new WhiteRealLiteralExpression(realToken);
		}

		public IWhiteRealLiteralExpression CreateWhiteRealLiteralExpression(string value)
		{
			return new WhiteRealLiteralExpression(TokenFactory<RealToken>.Create(value));
		}

		public IWhiteRangeExpression CreateWhiteRangeExpression(IWhiteExpression low, IWhiteExpression high)
		{
			return new WhiteRangeExpression(low, TokenFactory<IRangeToken>.Create(".."), high);
		}

		public IWhiteBoolLiteralExpression CreateWhiteBoolLiteralExpression(string value)
		{
			return new WhiteBoolLiteralExpression(TokenFactory<BooleanToken>.Create(value));
		}

		public IWhiteBoolLiteralExpression CreateWhiteBoolLiteralExpression(IBooleanToken boolean)
		{
			return new WhiteBoolLiteralExpression(boolean);
		}

		public IWhiteDurationLiteralExpression CreateWhiteDurationLiteralExpression(IDurationToken token)
		{
			return new WhiteDurationLiteralExpression(token);
		}

		public IWhiteDateLiteralExpression CreateWhiteDateLiteralExpression(IDateToken token)
		{
			return new WhiteDateLiteralExpression(token);
		}

		public IWhiteLDateLiteralExpression CreateWhiteLDateLiteralExpression(ILDateToken token)
		{
			return new WhiteLDateLiteralExpression(token);
		}

		public IWhiteLTimeOfDayLiteralExpression CreateWhiteLTimeOfDayLiteralExpression(ILTimeOfDayToken token)
		{
			return new WhiteLTimeOfDayLiteralExpression(token);
		}

		public IWhiteLDateAndTimeLiteralExpression CreateWhiteLDateAndTimeLiteralExpression(ILDateAndTimeToken token)
		{
			return new WhiteLDateAndTimeLiteralExpression(token);
		}

		public IWhiteDateAndTimeLiteralExpression CreateWhiteDateAndTimeLiteralExpression(IDateAndTimeToken token)
		{
			return new WhiteDateAndTimeLiteralExpression(token);
		}

		public IWhiteTimeOfDayLiteralExpression CreateWhiteTimeOfDayLiteralExpression(ITimeOfDayToken token)
		{
			return new WhiteTimeOfDayLiteralExpression(token);
		}

		public IWhiteLDurationLiteralExpression CreateWhiteLDurationLiteralExpression(ILDurationToken token)
		{
			return new WhiteLDurationLiteralExpression(token);
		}

		public IWhiteSingleByteStringLiteralExpression CreateWhiteSingleByteStringLiteralExpression(ISingleByteStringToken token)
		{
			return new WhiteSingleByteStringLiteralExpression(token);
		}

		public IWhiteDoubleByteStringLiteralExpression CreateWhiteDoubleByteStringLiteralExpression(IDoubleByteStringToken token)
		{
			return new WhiteDoubleByteStringLiteralExpression(token);
		}

		public IWhiteXStringLiteralExpression CreateWhiteXStringLiteralExpression(IXByteStringToken token)
		{
			return new WhiteXStringLiteralExpression(token);
		}

		public IWhiteDirectVariableExpression CreateWhiteDirectVariableExpression(IDirectVariableToken token)
		{
			return new WhiteDirectVariableExpression(token);
		}

		public IWhiteDeRefAccessExpression CreateWhiteDeRefAccessExpression(IWhiteExpression baseExpression, IDeRefToken token)
		{
			return new WhiteDeRefAccessExpression(baseExpression, token);
		}

		public IWhiteCompoAccessExpression CreateWhiteCompoAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IWhiteVariableExpression rightExpression)
		{
			return new WhiteCompoAccessExpression(leftExpression, period, rightExpression);
		}

		public IWhiteBitAccessExpression CreateWhiteBitAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IWhiteIntegerLiteralExpression rightExpression)
		{
			return new WhiteBitAccessExpression(leftExpression, period, rightExpression);
		}

		public IWhiteIndexAccessExpression CreateWhiteIndexAccessExpression(IWhiteExpression _base, ILeftBracketToken leftBracket, IEnumerable<IWhiteExpression> accesses, IRightBracketToken rightBracket)
		{
			return new WhiteIndexAccessExpression(_base, leftBracket, accesses, rightBracket);
		}

		public IParenthesizedExpression CreateParenthesizedExpression(ILeftParenthesisToken leftParenthesis, IWhiteExpression expression, IRightParenthesisToken rightParenthesis)
		{
			return new WhiteParenthesizedExpression(leftParenthesis, expression, rightParenthesis);
		}

		public IWhiteNamespaceAccessExpression CreateWhiteNamespaceAccessExpression(IWhiteExpression _namespace, IHashToken hashTag, IWhiteVariableExpression variableExpression)
		{
			return new WhiteNamespaceAccessExpression(_namespace, hashTag, variableExpression);
		}

		public IWhiteSpecialScopeExpression CreateWhiteSpecialScopeExpression(IWhiteScopeToken scope, IPeriodToken period, IWhiteExpression right)
		{
			return new WhiteSpecialScopeExpression(scope, period, right);
		}

		public IWhitePrefixedOperatorExpression CreateWhitePrefixedOperatorExpression(IWhitePrefixedOperatorToken _operator, ILeftParenthesisToken leftParenthesis, IEnumerable<IWhiteExpression> operands, IRightParenthesisToken rightParenthesis)
		{
			return new WhitePrefixedOperatorExpression(_operator, leftParenthesis, operands, rightParenthesis);
		}

		[Obsolete("No longer used. Use IWhitePrefixedOperatorExpression instead.")]
		public IWhiteSizeOfExpression CreateWhiteSizeOfExpression(IAnySizeOfToken sizeofOperator, ILeftParenthesisToken leftParenthesis, IWhiteTypeExpression typeExpression, IRightParenthesisToken rightParenthesis)
		{
			return new WhiteSizeOfExpression(sizeofOperator, leftParenthesis, typeExpression, rightParenthesis);
		}

		public IWhiteConversionExpression CreateWhiteConversionExpression(IConversionToken conversionOperator, ILeftParenthesisToken leftParenthesis, IWhiteExpression expression, IRightParenthesisToken rightParenthesis)
		{
			return new WhiteConversionExpression(conversionOperator, leftParenthesis, expression, rightParenthesis);
		}

		public IWhiteSimpleTypeExpression CreateWhiteSimpleTypeExpression(IWhiteSimpleTypeToken simpleTypeOperator)
		{
			return new WhiteSimpleTypeExpression(simpleTypeOperator);
		}

		public IWhiteUserDefTypeExpression CreateWhiteUserDefTypeExpression(IWhiteExpression nameExpression)
		{
			return new WhiteUserDefTypeExpression(nameExpression);
		}

		public IWhitePointerTypeExpression CreateWhitePointerTypeExpression(IPointerToken pointer, IToToken to, IWhiteTypeExpression baseType)
		{
			return new WhitePointerTypeExpression(pointer, to, baseType);
		}

		public IWhiteReferenceTypeExpression CreateWhiteReferenceTypeExpression(IReferenceToken reference, IToToken to, IWhiteTypeExpression baseType)
		{
			return new WhiteReferenceTypeExpression(reference, to, baseType);
		}

		public IWhiteArrayTypeExpression CreateWhiteArrayTypeExpression(IArrayToken array, ILeftBracketToken leftBracket, IEnumerable<IWhiteExpression> ranges, IRightBracketToken rightBracket, IOfToken of, IWhiteTypeExpression baseType)
		{
			return new WhiteArrayTypeExpression(array, leftBracket, ranges, rightBracket, of, baseType);
		}

		public ISubRangeTypeExpression CreateSubRangeTypeExpression(IWhiteTypeExpression baseType, ILeftParenthesisToken leftParenthesis, IWhiteRangeExpression range, IRightParenthesisToken rightParenthesis)
		{
			return new SubRangeTypeExpression(baseType, leftParenthesis, range, rightParenthesis);
		}

		public IStringTypeExpression CreateStringTypeExpression(IAnyStringSimpleTypeToken stringSimpleOperator, ILeftParenthesisToken leftParenthesis, IWhiteExpression length, IRightParenthesisToken rightParenthesis)
		{
			return new StringTypeExpression(stringSimpleOperator, leftParenthesis, length, rightParenthesis);
		}

		public IEnumerationTypeExpression CreateEnumerationTypeExpression(ILeftParenthesisToken leftParenthesis, IEnumerable<IWhiteExpression> definitions, IRightParenthesisToken rightParenthesis)
		{
			return new EnumerationTypeExpression(leftParenthesis, definitions, rightParenthesis);
		}

		public IVectorTypeExpression CreateVectorTypeExpression(I__VectorToken vector, ILeftBracketToken leftBracket, IWhiteExpression length, IRightBracketToken rightBracket, IOfToken of, IWhiteTypeExpression baseType)
		{
			return new VectorTypeExpression(vector, leftBracket, length, rightBracket, of, baseType);
		}

		public ILeadByCommaExpression CreateWhiteLeadByCommaExpression(IWhiteExpression expression)
		{
			return new WhiteLeadByCommaExpression(TokenFactory<ICommaToken>.Create(","), expression);
		}

		public IEmptyExpression CreateEmptyExpression()
		{
			return new WhiteEmptyExpression();
		}

		public IWhitePartialAccessExpression CreateWhitePartialAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IPartialAccessToken token)
		{
			return new WhitePartialAccessExpression(token);
		}

		public IWhiteCompoPartialAccessExpression CreateWhiteCompoPartialAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IWhitePartialAccessExpression rightExpression)
		{
			return new WhiteCompoPartialAccessExpression(leftExpression, period, rightExpression);
		}
	}
}
