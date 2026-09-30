using System;
using System.Runtime.CompilerServices;
using \u001C;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0008
{
	// Token: 0x02000277 RID: 631
	internal sealed class \u000F : AbstractToVisitchecker
	{
		// Token: 0x06002805 RID: 10245 RVA: 0x0008B798 File Offset: 0x00089998
		internal \u000F(\u0010 \u0018\u0006)
		{
			this.Replacer = \u0018\u0006;
		}

		// Token: 0x1700072F RID: 1839
		// (get) Token: 0x06002806 RID: 10246 RVA: 0x0008B7A8 File Offset: 0x000899A8
		private \u0010 Replacer { get; }

		// Token: 0x06002807 RID: 10247 RVA: 0x0008B7B0 File Offset: 0x000899B0
		public override bool ToVisit(_IAssignmentExpression assign)
		{
			return this.Replacer.\u0001(assign);
		}

		// Token: 0x04000768 RID: 1896
		[CompilerGenerated]
		private readonly \u0010 \u0001;
	}
}
