using System;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;

namespace \u0012
{
	// Token: 0x0200011C RID: 284
	internal sealed class \u0008 : global::\u000F.\u0006
	{
		// Token: 0x170004F6 RID: 1270
		// (get) Token: 0x06001491 RID: 5265 RVA: 0x0003CC44 File Offset: 0x0003AE44
		internal static global::\u000F.\u0006 Instance { get; } = new global::\u0012.\u0008();

		// Token: 0x06001492 RID: 5266 RVA: 0x0003CC4C File Offset: 0x0003AE4C
		private \u0008()
		{
		}

		// Token: 0x06001493 RID: 5267 RVA: 0x0003CC54 File Offset: 0x0003AE54
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			if (!\u0002.CompileContext.LibraryParamTablesEqual(\u0002.PreCompileContext) && \u0002.Strategy.LibraryParamTablesChanged())
			{
				return \u0002.Strategy.IsUpToDate;
			}
			return \u0002.CompileContext.LibraryListsEqual(\u0002.PreCompileContext, \u0002.PreCompileContextPool) || !\u0002.Strategy.LibraryListChanged() || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x0400038F RID: 911
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
