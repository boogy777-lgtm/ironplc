using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001DD RID: 477
	internal class EmptyStatement_Green : Statement_Green, _IEmptyStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IEmptyStatement
	{
		// Token: 0x060021AA RID: 8618 RVA: 0x00014CBD File Offset: 0x00013CBD
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021AB RID: 8619 RVA: 0x00014CCF File Offset: 0x00013CCF
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
