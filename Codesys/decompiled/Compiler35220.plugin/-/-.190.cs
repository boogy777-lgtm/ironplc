using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0014
{
	// Token: 0x02000225 RID: 549
	internal sealed class \u000E : EmptyVisitor351900
	{
		// Token: 0x06002445 RID: 9285 RVA: 0x0007C690 File Offset: 0x0007A890
		public static bool \u0001(_IExpression \u0002)
		{
			\u000E u000E = new \u000E();
			IStandardTraverser ivisit = new StandardTraverser(u000E);
			u000E.\u0001 = false;
			\u0002.Accept(ivisit);
			return u000E.\u0001;
		}

		// Token: 0x06002446 RID: 9286 RVA: 0x0007C6BC File Offset: 0x0007A8BC
		public override void visit(_IOperatorExpression op)
		{
			this.\u0001 = true;
		}

		// Token: 0x04000674 RID: 1652
		private bool \u0001;
	}
}
