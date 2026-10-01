using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E6 RID: 486
	internal class ContinueStatement_Green : Statement_Green, _IContinueStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IContinueStatement
	{
		// Token: 0x060021FB RID: 8699 RVA: 0x000149EC File Offset: 0x000139EC
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021FC RID: 8700 RVA: 0x000149FE File Offset: 0x000139FE
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
