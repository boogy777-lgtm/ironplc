using System;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x0200011A RID: 282
	internal sealed class \u0005 : global::\u000F.\u0006
	{
		// Token: 0x170004F4 RID: 1268
		// (get) Token: 0x06001487 RID: 5255 RVA: 0x0003CA68 File Offset: 0x0003AC68
		internal static global::\u000F.\u0006 Instance { get; } = new global::\u0001.\u0005();

		// Token: 0x06001488 RID: 5256 RVA: 0x0003CA70 File Offset: 0x0003AC70
		private \u0005()
		{
		}

		// Token: 0x06001489 RID: 5257 RVA: 0x0003CA78 File Offset: 0x0003AC78
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			return !global::\u0001.\u0005.\u0001(\u0002.CompileContext as _ICompileContext3) || !\u0002.Strategy.CompileOptionsChanged() || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x0600148A RID: 5258 RVA: 0x0003CAA8 File Offset: 0x0003ACA8
		private static bool \u0001(_ICompileContext3 \u0002)
		{
			return \u0002 != null && \u0002.CompileOptionsSavedWith != null && \u0002.CompileOptionsSavedWith.CompileOptionsChanged();
		}

		// Token: 0x0400038D RID: 909
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
