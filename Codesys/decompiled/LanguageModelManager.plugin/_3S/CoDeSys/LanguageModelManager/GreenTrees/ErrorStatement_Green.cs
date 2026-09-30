using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001DC RID: 476
	internal class ErrorStatement_Green : Statement_Green, IErrorStatement, IStatement, IExprement, _IErrorStatement, _IStatement, _IExprement, IExprement3, IExprement2
	{
		// Token: 0x060021A7 RID: 8615 RVA: 0x00014FF2 File Offset: 0x00013FF2
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021A8 RID: 8616 RVA: 0x00015004 File Offset: 0x00014004
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
