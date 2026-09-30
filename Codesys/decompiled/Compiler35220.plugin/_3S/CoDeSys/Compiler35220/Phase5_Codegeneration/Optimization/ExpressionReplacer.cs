using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x02000281 RID: 641
	public class ExpressionReplacer : EmptyVisitor352000, IExpressionReplacer
	{
		// Token: 0x06002897 RID: 10391 RVA: 0x0008E43C File Offset: 0x0008C63C
		public ExpressionReplacer(ISpecificExpressionReplacer replacer)
		{
			this.\u0001 = replacer;
		}

		// Token: 0x06002898 RID: 10392 RVA: 0x0008E44C File Offset: 0x0008C64C
		public _IExpression ReplaceExpression(_IExpression expression, bool bReadAccess = true)
		{
			this.\u0001 = expression;
			this.\u0001 = bReadAccess;
			expression.Accept(this);
			return this.\u0001;
		}

		// Token: 0x06002899 RID: 10393 RVA: 0x0008E46C File Offset: 0x0008C66C
		public override void visit(_IVariableExpression varExp)
		{
			this.\u0001 = this.\u0001.ReplaceVariableExpression(varExp, this.\u0001);
		}

		// Token: 0x0600289A RID: 10394 RVA: 0x0008E488 File Offset: 0x0008C688
		public override void visit(_IIndexAccessExpression indexaccess)
		{
			this.\u0001 = this.\u0001.ReplaceIndexAccessExpression(indexaccess, this.\u0001);
		}

		// Token: 0x0600289B RID: 10395 RVA: 0x0008E4A4 File Offset: 0x0008C6A4
		public override void visit(_IDeRefAccessExpression deref)
		{
			this.\u0001 = this.\u0001.ReplaceDeRefAccessExpression(deref, this.\u0001);
		}

		// Token: 0x0600289C RID: 10396 RVA: 0x0008E4C0 File Offset: 0x0008C6C0
		public override void visit(_IAssignmentExpression assign)
		{
			this.\u0001 = this.\u0001.ReplaceAssignmentExpression(assign);
		}

		// Token: 0x0600289D RID: 10397 RVA: 0x0008E4D4 File Offset: 0x0008C6D4
		public override void visit(_IOperatorExpression op)
		{
			this.\u0001 = this.\u0001.ReplaceOperatorExpression(op);
		}

		// Token: 0x0600289E RID: 10398 RVA: 0x0008E4E8 File Offset: 0x0008C6E8
		public override void visit(_IConversionExpression conv)
		{
			this.\u0001 = this.\u0001.ReplaceConversionExpression(conv);
		}

		// Token: 0x0600289F RID: 10399 RVA: 0x0008E4FC File Offset: 0x0008C6FC
		public override void visit(_ICompoAccessExpression compoExp)
		{
			this.\u0001 = this.\u0001.ReplaceCompoAccessExpression(compoExp, this.\u0001);
		}

		// Token: 0x060028A0 RID: 10400 RVA: 0x0008E518 File Offset: 0x0008C718
		public override void visit(_IThisExpression thisexp)
		{
			this.\u0001 = this.\u0001.ReplaceThisExpression(thisexp);
		}

		// Token: 0x060028A1 RID: 10401 RVA: 0x0008E52C File Offset: 0x0008C72C
		public override void visit(_IBaseExpression baseexp)
		{
			this.\u0001 = this.\u0001.ReplaceBaseExpression(baseexp);
		}

		// Token: 0x060028A2 RID: 10402 RVA: 0x0008E540 File Offset: 0x0008C740
		public override void visit(_ICurrentTaskExpression currentTaskExp)
		{
			this.\u0001 = this.\u0001.ReplaceCurrentTaskExpression(currentTaskExp);
		}

		// Token: 0x060028A3 RID: 10403 RVA: 0x0008E554 File Offset: 0x0008C754
		public override void visit(_ICallExpression call)
		{
			this.\u0001 = this.\u0001.ReplaceCallExpression(call);
		}

		// Token: 0x060028A4 RID: 10404 RVA: 0x0008E568 File Offset: 0x0008C768
		public override void visit(_IPartialAccessExpression partialAccessExpression)
		{
			this.\u0001 = this.\u0001.ReplacePartialAccessExpression(partialAccessExpression, this.\u0001);
		}

		// Token: 0x04000774 RID: 1908
		private readonly ISpecificExpressionReplacer \u0001;

		// Token: 0x04000775 RID: 1909
		private bool \u0001;

		// Token: 0x04000776 RID: 1910
		private _IExpression \u0001;
	}
}
