using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F7 RID: 503
	internal class ErrorExpression_Green : Expression_Green, _IErrorExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IErrorExpression
	{
		// Token: 0x0600227C RID: 8828 RVA: 0x0000E533 File Offset: 0x0000D533
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600227D RID: 8829 RVA: 0x0000E545 File Offset: 0x0000D545
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor6 exprVisitor = visitor as IExprVisitor6;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}
	}
}
