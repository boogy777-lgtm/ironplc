using System;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;

namespace \u001C
{
	// Token: 0x02000119 RID: 281
	internal sealed class \u0006 : global::\u000F.\u0006
	{
		// Token: 0x170004F3 RID: 1267
		// (get) Token: 0x06001483 RID: 5251 RVA: 0x0003CA1C File Offset: 0x0003AC1C
		internal static global::\u000F.\u0006 Instance { get; } = new \u001C.\u0006();

		// Token: 0x06001484 RID: 5252 RVA: 0x0003CA24 File Offset: 0x0003AC24
		private \u0006()
		{
		}

		// Token: 0x06001485 RID: 5253 RVA: 0x0003CA2C File Offset: 0x0003AC2C
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			return !\u0002.CompileContext.DefineChanged(\u0002.PreCompileContext) || !\u0002.Strategy.DefinesChanged() || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x0400038C RID: 908
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
