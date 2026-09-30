using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IExpressionSyntax
	{
		public interface IExpressionVisitor
		{
			void visit(IWhiteAssignmentExpression expression);

			void visit(IWhiteBinaryOperatorExpression expression);

			void visit(IWhitePrefixedOperatorExpression expression);

			void visit(IWhiteUnaryOperatorExpression expression);

			void visit(IWhiteVariableExpression expression);

			void visit(IWhiteCallExpression expression);

			void visit(IWhiteArrayInitializationExpression expression);

			void visit(IWhiteMultipleIndexInitialization expression);

			void visit(ILeadByCommaExpression expression);

			void visit(IWhiteRangeExpression expression);

			void visit(IWhiteIntegerLiteralExpression expression);

			void visit(IWhiteRealLiteralExpression expression);

			void visit(IWhiteBoolLiteralExpression expression);

			void visit(IWhiteDurationLiteralExpression expression);

			void visit(IWhiteDateLiteralExpression expression);

			void visit(IWhiteLDateLiteralExpression expression);

			void visit(IWhiteLTimeOfDayLiteralExpression expression);

			void visit(IWhiteLDateAndTimeLiteralExpression expression);

			void visit(IWhiteDateAndTimeLiteralExpression expression);

			void visit(IWhiteTimeOfDayLiteralExpression expression);

			void visit(IWhiteLDurationLiteralExpression expression);

			void visit(IWhiteSingleByteStringLiteralExpression expression);

			void visit(IWhiteDoubleByteStringLiteralExpression expression);

			void visit(IWhiteXStringLiteralExpression expression);

			void visit(IWhiteDirectVariableExpression expression);

			void visit(IWhiteIncompleteDirectVariableExpression expression);

			void visit(IWhiteDeRefAccessExpression expression);

			void visit(IWhiteCompoAccessExpression expression);

			void visit(IWhiteBitAccessExpression expression);

			void visit(IWhiteIndexAccessExpression expression);

			void visit(IParenthesizedExpression expression);

			void visit(IWhiteThisExpression expression);

			void visit(IWhiteSuperExpression expression);

			void visit(IWhiteNamespaceAccessExpression expression);

			void visit(IWhiteSpecialScopeExpression expression);

			void visit(IWhiteGlobalScopeExpression expression);

			void visit(IWhiteConversionExpression expression);

			void visit(IWhiteTypeExpression expression);

			void visit(IWhiteSimpleTypeExpression expression);

			void visit(IWhiteUserDefTypeExpression expression);

			void visit(IWhitePointerTypeExpression expression);

			void visit(IWhiteReferenceTypeExpression expression);

			void visit(IWhiteArrayTypeExpression expression);

			void visit(ISubRangeTypeExpression expression);

			void visit(IStringTypeExpression expression);

			void visit(IEnumerationTypeExpression expression);

			void visit(IVectorTypeExpression expression);

			void visit(IWhiteSizeOfExpression expression);

			void visit(IEmptyExpression expression);

			void visit(IWhiteStructureInitialization expression);
		}

		public interface IExpressionVisitor<[Nullable(2)] out T>
		{
			T visit(IWhiteAssignmentExpression expression);

			T visit(IWhiteBinaryOperatorExpression expression);

			T visit(IWhitePrefixedOperatorExpression expression);

			T visit(IWhiteUnaryOperatorExpression expression);

			T visit(IWhiteMultipleIndexInitialization expression);

			T visit(IWhiteVariableExpression expression);

			T visit(IWhiteCallExpression expression);

			T visit(ILeadByCommaExpression expression);

			T visit(IWhiteRangeExpression expression);

			T visit(IWhiteIntegerLiteralExpression expression);

			T visit(IWhiteRealLiteralExpression expression);

			T visit(IWhiteBoolLiteralExpression expression);

			T visit(IWhiteDurationLiteralExpression expression);

			T visit(IWhiteDateLiteralExpression expression);

			T visit(IWhiteLDateLiteralExpression expression);

			T visit(IWhiteLTimeOfDayLiteralExpression expression);

			T visit(IWhiteLDateAndTimeLiteralExpression expression);

			T visit(IWhiteDateAndTimeLiteralExpression expression);

			T visit(IWhiteTimeOfDayLiteralExpression expression);

			T visit(IWhiteLDurationLiteralExpression expression);

			T visit(IWhiteSingleByteStringLiteralExpression expression);

			T visit(IWhiteDoubleByteStringLiteralExpression expression);

			T visit(IWhiteXStringLiteralExpression expression);

			T visit(IWhiteDirectVariableExpression expression);

			T visit(IWhiteIncompleteDirectVariableExpression expression);

			T visit(IWhiteArrayInitializationExpression expression);

			T visit(IWhiteDeRefAccessExpression expression);

			T visit(IWhiteCompoAccessExpression expression);

			T visit(IWhiteBitAccessExpression expression);

			T visit(IWhiteIndexAccessExpression expression);

			T visit(IParenthesizedExpression expression);

			T visit(IWhiteThisExpression expression);

			T visit(IWhiteSuperExpression expression);

			T visit(IWhiteNamespaceAccessExpression expression);

			T visit(IWhiteSpecialScopeExpression expression);

			T visit(IWhiteGlobalScopeExpression expression);

			T visit(IWhiteConversionExpression expression);

			T visit(IWhiteTypeExpression expression);

			T visit(IWhiteSimpleTypeExpression expression);

			T visit(IWhiteUserDefTypeExpression expression);

			T visit(IWhitePointerTypeExpression expression);

			T visit(IWhiteReferenceTypeExpression expression);

			T visit(IWhiteArrayTypeExpression expression);

			T visit(ISubRangeTypeExpression expression);

			T visit(IStringTypeExpression expression);

			T visit(IEnumerationTypeExpression expression);

			T visit(IVectorTypeExpression expression);

			T visit(IWhiteSizeOfExpression expression);

			T visit(IEmptyExpression expression);

			T visit(IWhiteStructureInitialization expression);
		}

		public interface IExpressionVisitor<[Nullable(2)] out T, [Nullable(2)] in TContext>
		{
			T visit(IWhiteAssignmentExpression expression, TContext context);

			T visit(IWhiteBinaryOperatorExpression expression, TContext context);

			T visit(IWhitePrefixedOperatorExpression expression, TContext context);

			T visit(IWhiteUnaryOperatorExpression expression, TContext context);

			T visit(IWhiteVariableExpression expression, TContext context);

			T visit(IWhiteCallExpression expression, TContext context);

			T visit(ILeadByCommaExpression expression, TContext context);

			T visit(IWhiteRangeExpression expression, TContext context);

			T visit(IWhiteIntegerLiteralExpression expression, TContext context);

			T visit(IWhiteRealLiteralExpression expression, TContext context);

			T visit(IWhiteBoolLiteralExpression expression, TContext context);

			T visit(IWhiteDurationLiteralExpression expression, TContext context);

			T visit(IWhiteMultipleIndexInitialization expression, TContext context);

			T visit(IWhiteDateLiteralExpression expression, TContext context);

			T visit(IWhiteLDateLiteralExpression expression, TContext context);

			T visit(IWhiteLTimeOfDayLiteralExpression expression, TContext context);

			T visit(IWhiteLDateAndTimeLiteralExpression expression, TContext context);

			T visit(IWhiteDateAndTimeLiteralExpression expression, TContext context);

			T visit(IWhiteTimeOfDayLiteralExpression expression, TContext context);

			T visit(IWhiteLDurationLiteralExpression expression, TContext context);

			T visit(IWhiteSingleByteStringLiteralExpression expression, TContext context);

			T visit(IWhiteDoubleByteStringLiteralExpression expression, TContext context);

			T visit(IWhiteXStringLiteralExpression expression, TContext context);

			T visit(IWhiteDirectVariableExpression expression, TContext context);

			T visit(IWhiteIncompleteDirectVariableExpression expression, TContext context);

			T visit(IWhiteDeRefAccessExpression expression, TContext context);

			T visit(IWhiteCompoAccessExpression expression, TContext context);

			T visit(IWhiteBitAccessExpression expression, TContext context);

			T visit(IWhiteIndexAccessExpression expression, TContext context);

			T visit(IParenthesizedExpression expression, TContext context);

			T visit(IWhiteThisExpression expression, TContext context);

			T visit(IWhiteSuperExpression expression, TContext context);

			T visit(IWhiteNamespaceAccessExpression expression, TContext context);

			T visit(IWhiteSpecialScopeExpression expression, TContext context);

			T visit(IWhiteGlobalScopeExpression expression, TContext context);

			T visit(IWhiteConversionExpression expression, TContext context);

			T visit(IWhiteTypeExpression expression, TContext context);

			T visit(IWhiteSimpleTypeExpression expression, TContext context);

			T visit(IWhiteUserDefTypeExpression expression, TContext context);

			T visit(IWhitePointerTypeExpression expression, TContext context);

			T visit(IWhiteReferenceTypeExpression expression, TContext context);

			T visit(IWhiteArrayInitializationExpression expression, TContext context);

			T visit(IWhiteArrayTypeExpression expression, TContext context);

			T visit(ISubRangeTypeExpression expression, TContext context);

			T visit(IStringTypeExpression expression, TContext context);

			T visit(IEnumerationTypeExpression expression, TContext context);

			T visit(IVectorTypeExpression expression, TContext context);

			T visit(IWhiteSizeOfExpression expression, TContext context);

			T visit(IEmptyExpression expression, TContext context);

			T visit(IWhiteStructureInitialization expression, TContext context);
		}

		void Accept(IExpressionVisitor visitor);

		T Accept<[Nullable(2)] T>(IExpressionVisitor<T> visitor);

		T Accept<[Nullable(2)] T, [Nullable(2)] TContext>(IExpressionVisitor<T, TContext> visitor, TContext context);
	}
}
