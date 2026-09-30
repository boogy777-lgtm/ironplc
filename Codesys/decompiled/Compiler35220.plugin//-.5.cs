using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0081
{
	// Token: 0x02000107 RID: 263
	internal sealed class \u0005 : StandardTraverser
	{
		// Token: 0x060013BF RID: 5055 RVA: 0x00038D1C File Offset: 0x00036F1C
		public \u0005(IExprementVisitorNoTraversion351300 \u009D) : base(\u009D)
		{
		}

		// Token: 0x060013C0 RID: 5056 RVA: 0x00038D28 File Offset: 0x00036F28
		public override void visit(_IOperatorExpression op)
		{
			if (op.Code == Operator.__TypeOf)
			{
				return;
			}
			base.visit(op);
		}
	}
}
