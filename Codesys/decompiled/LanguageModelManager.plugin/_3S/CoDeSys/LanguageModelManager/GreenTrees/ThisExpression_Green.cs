using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000200 RID: 512
	internal class ThisExpression_Green : Expression_Green, _IThisExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IThisExpression
	{
		// Token: 0x060022F9 RID: 8953 RVA: 0x00012EE3 File Offset: 0x00011EE3
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060022FA RID: 8954 RVA: 0x00012EF5 File Offset: 0x00011EF5
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
