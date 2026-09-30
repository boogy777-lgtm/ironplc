using System;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;

namespace \u0080
{
	// Token: 0x02000117 RID: 279
	internal sealed class \u0006 : global::\u000F.\u0006
	{
		// Token: 0x170004F2 RID: 1266
		// (get) Token: 0x0600147E RID: 5246 RVA: 0x0003C9CC File Offset: 0x0003ABCC
		internal static global::\u000F.\u0006 Instance { get; } = new \u0080.\u0006();

		// Token: 0x0600147F RID: 5247 RVA: 0x0003C9D4 File Offset: 0x0003ABD4
		private \u0006()
		{
		}

		// Token: 0x06001480 RID: 5248 RVA: 0x0003C9DC File Offset: 0x0003ABDC
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			return \u0002.CompileContext.SimulationMode == \u0002.PreCompileContext.SimulationMode || !\u0002.Strategy.SimulationModeChanged() || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x0400038B RID: 907
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
