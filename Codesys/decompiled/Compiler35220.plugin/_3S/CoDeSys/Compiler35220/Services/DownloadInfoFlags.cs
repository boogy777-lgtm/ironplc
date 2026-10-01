using System;
using System.Runtime.CompilerServices;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000E5 RID: 229
	public struct DownloadInfoFlags
	{
		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x0600100F RID: 4111 RVA: 0x0002CE60 File Offset: 0x0002B060
		// (set) Token: 0x06001010 RID: 4112 RVA: 0x0002CE68 File Offset: 0x0002B068
		public bool OnlineChange { get; set; }

		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x06001011 RID: 4113 RVA: 0x0002CE74 File Offset: 0x0002B074
		// (set) Token: 0x06001012 RID: 4114 RVA: 0x0002CE7C File Offset: 0x0002B07C
		public bool BootProject { get; set; }

		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x06001013 RID: 4115 RVA: 0x0002CE88 File Offset: 0x0002B088
		// (set) Token: 0x06001014 RID: 4116 RVA: 0x0002CE90 File Offset: 0x0002B090
		public bool OfflineBootProject { get; set; }

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x06001015 RID: 4117 RVA: 0x0002CE9C File Offset: 0x0002B09C
		// (set) Token: 0x06001016 RID: 4118 RVA: 0x0002CEA4 File Offset: 0x0002B0A4
		public bool CompactDownload { get; set; }

		// Token: 0x040002D6 RID: 726
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040002D7 RID: 727
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x040002D8 RID: 728
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x040002D9 RID: 729
		[CompilerGenerated]
		private bool \u0004;
	}
}
