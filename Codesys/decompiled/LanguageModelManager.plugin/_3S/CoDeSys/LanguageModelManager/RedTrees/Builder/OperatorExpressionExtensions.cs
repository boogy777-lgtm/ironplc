using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Expressions;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder
{
	// Token: 0x02000264 RID: 612
	public static class OperatorExpressionExtensions
	{
		// Token: 0x060029CA RID: 10698 RVA: 0x0006AABD File Offset: 0x00069ABD
		private static IOperatorExpression Into(this IExpression left, Operator op, IExpression right)
		{
			return OperatorBuilder.Init().Lhs(left).Op(op).Rhs(right).Build();
		}

		// Token: 0x060029CB RID: 10699 RVA: 0x0006AADB File Offset: 0x00069ADB
		public static IOperatorExpression Equal(this IExpression lhs, IExpression rhs)
		{
			return lhs.Into(Operator.Equal, rhs);
		}

		// Token: 0x060029CC RID: 10700 RVA: 0x0006AAE9 File Offset: 0x00069AE9
		public static IOperatorExpression NotEqual(this IExpression lhs, IExpression rhs)
		{
			return lhs.Into(Operator.NotEqual, rhs);
		}

		// Token: 0x060029CD RID: 10701 RVA: 0x0006AAF7 File Offset: 0x00069AF7
		public static IOperatorExpression Greater(this IExpression lhs, IExpression rhs)
		{
			return lhs.Into(Operator.Greater, rhs);
		}

		// Token: 0x060029CE RID: 10702 RVA: 0x0006AB05 File Offset: 0x00069B05
		public static IOperatorExpression GreaterEqual(this IExpression lhs, IExpression rhs)
		{
			return lhs.Into(Operator.GreaterEqual, rhs);
		}

		// Token: 0x060029CF RID: 10703 RVA: 0x0006AB13 File Offset: 0x00069B13
		public static IOperatorExpression Less(this IExpression lhs, IExpression rhs)
		{
			return lhs.Into(Operator.Less, rhs);
		}

		// Token: 0x060029D0 RID: 10704 RVA: 0x0006AB21 File Offset: 0x00069B21
		public static IOperatorExpression LessEqual(this IExpression lhs, IExpression rhs)
		{
			return lhs.Into(Operator.LessEqual, rhs);
		}
	}
}
