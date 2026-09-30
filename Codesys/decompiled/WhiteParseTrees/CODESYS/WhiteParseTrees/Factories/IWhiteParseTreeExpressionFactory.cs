using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteParseTreeExpressionFactory
	{
		T ParseExpression<[Nullable(0)] T>(string stInput) where T : IWhiteExpression;

		[return: Nullable(2)]
		IWhiteExpression ParseExpression(string stInput);

		IWhiteAssignmentExpression CreateWhiteAssignmentExpression(IWhiteExpression expLValue, IWhiteExpression expRValue, IAnyAssignmentToken assignment);

		IWhiteBinaryOperatorExpression CreateWhiteBinaryOperatorExpression(IWhiteExpression first, IWhiteOperatorToken token, IWhiteExpression second);

		IWhiteUnaryOperatorExpression CreateWhiteUnaryOperatorExpression(IWhiteOperatorToken token, IWhiteExpression operand);

		IWhiteVariableExpression CreateWhiteVariableExpression(string value);

		IWhiteVariableExpression CreateWhiteVariableExpression(IIdentifierToken token);

		IWhiteIntegerLiteralExpression CreateWhiteIntegerLiteralExpression(IIntegerToken intToken);

		IWhiteIntegerLiteralExpression CreateWhiteIntegerLiteralExpression(int value);

		IWhiteRealLiteralExpression CreateWhiteRealLiteralExpression(IRealToken realToken);

		IWhiteRealLiteralExpression CreateWhiteRealLiteralExpression(string value);

		IWhiteRangeExpression CreateWhiteRangeExpression(IWhiteExpression low, IWhiteExpression high);

		IWhiteBoolLiteralExpression CreateWhiteBoolLiteralExpression(string value);

		IWhiteBoolLiteralExpression CreateWhiteBoolLiteralExpression(IBooleanToken boolean);

		IWhiteDurationLiteralExpression CreateWhiteDurationLiteralExpression(IDurationToken token);

		IWhiteDateLiteralExpression CreateWhiteDateLiteralExpression(IDateToken token);

		IWhiteLDateLiteralExpression CreateWhiteLDateLiteralExpression(ILDateToken token);

		IWhiteLTimeOfDayLiteralExpression CreateWhiteLTimeOfDayLiteralExpression(ILTimeOfDayToken token);

		IWhiteLDateAndTimeLiteralExpression CreateWhiteLDateAndTimeLiteralExpression(ILDateAndTimeToken token);

		IWhiteDateAndTimeLiteralExpression CreateWhiteDateAndTimeLiteralExpression(IDateAndTimeToken token);

		IWhiteTimeOfDayLiteralExpression CreateWhiteTimeOfDayLiteralExpression(ITimeOfDayToken token);

		IWhiteLDurationLiteralExpression CreateWhiteLDurationLiteralExpression(ILDurationToken token);

		IWhiteSingleByteStringLiteralExpression CreateWhiteSingleByteStringLiteralExpression(ISingleByteStringToken token);

		IWhiteDoubleByteStringLiteralExpression CreateWhiteDoubleByteStringLiteralExpression(IDoubleByteStringToken token);

		IWhiteXStringLiteralExpression CreateWhiteXStringLiteralExpression(IXByteStringToken token);

		IWhiteDirectVariableExpression CreateWhiteDirectVariableExpression(IDirectVariableToken token);

		IWhiteDeRefAccessExpression CreateWhiteDeRefAccessExpression(IWhiteExpression baseExpression, IDeRefToken token);

		IWhiteCompoAccessExpression CreateWhiteCompoAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IWhiteVariableExpression rightExpression);

		IWhiteBitAccessExpression CreateWhiteBitAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IWhiteIntegerLiteralExpression rightExpression);

		IWhiteIndexAccessExpression CreateWhiteIndexAccessExpression(IWhiteExpression _base, ILeftBracketToken leftBracket, IEnumerable<IWhiteExpression> accesses, IRightBracketToken rightBracket);

		IParenthesizedExpression CreateParenthesizedExpression(ILeftParenthesisToken leftParenthesis, IWhiteExpression expression, IRightParenthesisToken rightParenthesis);

		IWhiteNamespaceAccessExpression CreateWhiteNamespaceAccessExpression(IWhiteExpression _namespace, IHashToken hashTag, IWhiteVariableExpression variableExpression);

		IWhiteSpecialScopeExpression CreateWhiteSpecialScopeExpression(IWhiteScopeToken scope, IPeriodToken period, IWhiteExpression right);

		IWhitePrefixedOperatorExpression CreateWhitePrefixedOperatorExpression(IWhitePrefixedOperatorToken _operator, ILeftParenthesisToken leftParenthesis, IEnumerable<IWhiteExpression> operands, IRightParenthesisToken rightParenthesis);

		[Obsolete("No longer used. The parser uses IWhitePrefixedOperatorExpression instead.")]
		IWhiteSizeOfExpression CreateWhiteSizeOfExpression(IAnySizeOfToken sizeofOperator, ILeftParenthesisToken leftParenthesis, IWhiteTypeExpression typeExpression, IRightParenthesisToken rightParenthesis);

		IWhiteConversionExpression CreateWhiteConversionExpression(IConversionToken conversionOperator, ILeftParenthesisToken leftParenthesis, IWhiteExpression expression, IRightParenthesisToken rightParenthesis);

		IWhiteSimpleTypeExpression CreateWhiteSimpleTypeExpression(IWhiteSimpleTypeToken simpleTypeOperator);

		IWhiteUserDefTypeExpression CreateWhiteUserDefTypeExpression(IWhiteExpression nameExpression);

		IWhitePointerTypeExpression CreateWhitePointerTypeExpression(IPointerToken pointer, IToToken to, IWhiteTypeExpression baseType);

		IWhiteReferenceTypeExpression CreateWhiteReferenceTypeExpression(IReferenceToken reference, IToToken to, IWhiteTypeExpression baseType);

		IWhiteArrayTypeExpression CreateWhiteArrayTypeExpression(IArrayToken array, ILeftBracketToken leftBracket, IEnumerable<IWhiteExpression> ranges, IRightBracketToken rightBracket, IOfToken of, IWhiteTypeExpression baseType);

		ISubRangeTypeExpression CreateSubRangeTypeExpression(IWhiteTypeExpression baseType, ILeftParenthesisToken leftParenthesis, IWhiteRangeExpression range, IRightParenthesisToken rightParenthesis);

		IStringTypeExpression CreateStringTypeExpression(IAnyStringSimpleTypeToken stringSimpleOperator, ILeftParenthesisToken leftParenthesis, IWhiteExpression length, IRightParenthesisToken rightParenthesis);

		IEnumerationTypeExpression CreateEnumerationTypeExpression(ILeftParenthesisToken leftParenthesis, IEnumerable<IWhiteExpression> definitions, IRightParenthesisToken rightParenthesis);

		IVectorTypeExpression CreateVectorTypeExpression(I__VectorToken vector, ILeftBracketToken leftBracket, IWhiteExpression length, IRightBracketToken rightBracket, IOfToken of, IWhiteTypeExpression baseType);

		ILeadByCommaExpression CreateWhiteLeadByCommaExpression(IWhiteExpression expression);

		IEmptyExpression CreateEmptyExpression();
	}
}
