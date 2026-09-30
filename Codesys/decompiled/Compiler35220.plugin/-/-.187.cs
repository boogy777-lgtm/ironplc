using System;
using System.Runtime.CompilerServices;

namespace \u0019
{
	// Token: 0x02000215 RID: 533
	internal sealed class \u0006
	{
		// Token: 0x06002320 RID: 8992 RVA: 0x00078F5C File Offset: 0x0007715C
		public \u0006(int \u0008\u0004)
		{
			this.CurrentBPIndex = \u0008\u0004;
			this.CurrentExceptionHandlingIndex = -1;
		}

		// Token: 0x17000683 RID: 1667
		// (get) Token: 0x06002321 RID: 8993 RVA: 0x00078F74 File Offset: 0x00077174
		// (set) Token: 0x06002322 RID: 8994 RVA: 0x00078F7C File Offset: 0x0007717C
		public int CurrentBPIndex { get; set; }

		// Token: 0x17000684 RID: 1668
		// (get) Token: 0x06002323 RID: 8995 RVA: 0x00078F88 File Offset: 0x00077188
		// (set) Token: 0x06002324 RID: 8996 RVA: 0x00078F90 File Offset: 0x00077190
		public int CurrentExceptionHandlingIndex { get; set; }

		// Token: 0x17000685 RID: 1669
		// (get) Token: 0x06002325 RID: 8997 RVA: 0x00078F9C File Offset: 0x0007719C
		// (set) Token: 0x06002326 RID: 8998 RVA: 0x00078FA4 File Offset: 0x000771A4
		public bool DoBP { get; set; }

		// Token: 0x0400062E RID: 1582
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x0400062F RID: 1583
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x04000630 RID: 1584
		[CompilerGenerated]
		private bool \u0001;
	}
}
