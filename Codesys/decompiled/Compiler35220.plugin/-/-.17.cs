using System;
using \u0001;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001D
{
	// Token: 0x0200006F RID: 111
	internal sealed class \u0001 : StandardTraverser
	{
		// Token: 0x060008C4 RID: 2244 RVA: 0x0001212C File Offset: 0x0001032C
		internal \u0001(\u0012 \u009C) : base(\u009C)
		{
			this.\u0001 = \u009C;
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x0001213C File Offset: 0x0001033C
		public override void visit(_ICallExpression call)
		{
			bool u = this.\u0001.InCall;
			this.\u0001.InCall = true;
			base.visit(call);
			this.\u0001.InCall = u;
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x00012174 File Offset: 0x00010374
		public override void visit(_IOperatorExpression op)
		{
			if (op.Code != Operator.__TypeOf && !Helper.\u0001(op) && op.Code != Operator.__LocalOffset)
			{
				base.visit(op);
			}
		}

		// Token: 0x0400012F RID: 303
		private \u0012 \u0001;
	}
}
