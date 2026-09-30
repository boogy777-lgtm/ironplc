using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F5 RID: 501
	internal class NullStatement_Green : Statement_Green, _INullStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x0600226E RID: 8814 RVA: 0x00015940 File Offset: 0x00014940
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600226F RID: 8815 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}
	}
}
