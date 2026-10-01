using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E5 RID: 485
	internal class ExitStatement_Green : Statement_Green, _IExitStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IExitStatement
	{
		// Token: 0x060021F8 RID: 8696 RVA: 0x00015033 File Offset: 0x00014033
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021F9 RID: 8697 RVA: 0x00015045 File Offset: 0x00014045
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
