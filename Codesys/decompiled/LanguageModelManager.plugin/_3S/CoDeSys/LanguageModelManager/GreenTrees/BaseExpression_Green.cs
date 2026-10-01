using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000201 RID: 513
	internal class BaseExpression_Green : Expression_Green, _IBaseExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IBaseExpression
	{
		// Token: 0x060022FC RID: 8956 RVA: 0x0000BA04 File Offset: 0x0000AA04
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060022FD RID: 8957 RVA: 0x0000BA16 File Offset: 0x0000AA16
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
