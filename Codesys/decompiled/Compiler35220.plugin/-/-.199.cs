using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0018
{
	// Token: 0x02000234 RID: 564
	internal class \u0005 : IExprInfo
	{
		// Token: 0x170006CD RID: 1741
		// (get) Token: 0x06002552 RID: 9554 RVA: 0x00081C40 File Offset: 0x0007FE40
		// (set) Token: 0x06002553 RID: 9555 RVA: 0x00081C48 File Offset: 0x0007FE48
		public bool DoGeneration { get; set; } = true;

		// Token: 0x170006CE RID: 1742
		// (get) Token: 0x06002554 RID: 9556 RVA: 0x00081C54 File Offset: 0x0007FE54
		// (set) Token: 0x06002555 RID: 9557 RVA: 0x00081C5C File Offset: 0x0007FE5C
		public int NestingDepth { get; set; }

		// Token: 0x170006CF RID: 1743
		// (get) Token: 0x06002556 RID: 9558 RVA: 0x00081C68 File Offset: 0x0007FE68
		// (set) Token: 0x06002557 RID: 9559 RVA: 0x00081C70 File Offset: 0x0007FE70
		public bool HasSideEffect { get; set; }

		// Token: 0x040006B0 RID: 1712
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040006B1 RID: 1713
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040006B2 RID: 1714
		[CompilerGenerated]
		private bool \u0002;
	}
}
