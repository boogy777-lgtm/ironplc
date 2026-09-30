using System;
using System.Runtime.CompilerServices;
using \u0017;
using \u001D;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0011
{
	// Token: 0x02000230 RID: 560
	internal sealed class \u0007
	{
		// Token: 0x06002524 RID: 9508 RVA: 0x00081944 File Offset: 0x0007FB44
		public void \u0001()
		{
			this.Offset = 0;
			this.CurrentType = null;
			this.BitOffset = byte.MaxValue;
			this.AccessModeFlags = AccessModeFlags.Read;
			this.SignatureId = Helper.InvalidId;
			this.VirtualFunctionCall = false;
			this.IndexInfo = null;
			this.VarExpInfo.\u0001();
			this.Callee = null;
			this.CurrentScratchOffset = 0;
			this.NoReferenceDeRef = false;
			this.CurrentPackMode = -1;
		}

		// Token: 0x170006BC RID: 1724
		// (get) Token: 0x06002525 RID: 9509 RVA: 0x000819B4 File Offset: 0x0007FBB4
		// (set) Token: 0x06002526 RID: 9510 RVA: 0x000819BC File Offset: 0x0007FBBC
		public bool NoReferenceDeRef { get; set; }

		// Token: 0x170006BD RID: 1725
		// (get) Token: 0x06002527 RID: 9511 RVA: 0x000819C8 File Offset: 0x0007FBC8
		// (set) Token: 0x06002528 RID: 9512 RVA: 0x000819D0 File Offset: 0x0007FBD0
		public \u001D.\u0007 VarExpInfo { get; set; } = \u001D.\u0007.\u0001();

		// Token: 0x170006BE RID: 1726
		// (get) Token: 0x06002529 RID: 9513 RVA: 0x000819DC File Offset: 0x0007FBDC
		// (set) Token: 0x0600252A RID: 9514 RVA: 0x000819E4 File Offset: 0x0007FBE4
		public int Offset { get; set; }

		// Token: 0x170006BF RID: 1727
		// (get) Token: 0x0600252B RID: 9515 RVA: 0x000819F0 File Offset: 0x0007FBF0
		// (set) Token: 0x0600252C RID: 9516 RVA: 0x000819F8 File Offset: 0x0007FBF8
		public IExpression Callee { get; set; }

		// Token: 0x170006C0 RID: 1728
		// (get) Token: 0x0600252D RID: 9517 RVA: 0x00081A04 File Offset: 0x0007FC04
		// (set) Token: 0x0600252E RID: 9518 RVA: 0x00081A0C File Offset: 0x0007FC0C
		public AccessModeFlags AccessModeFlags { get; set; } = AccessModeFlags.Read;

		// Token: 0x170006C1 RID: 1729
		// (get) Token: 0x0600252F RID: 9519 RVA: 0x00081A18 File Offset: 0x0007FC18
		// (set) Token: 0x06002530 RID: 9520 RVA: 0x00081A20 File Offset: 0x0007FC20
		public ICompiledType CurrentType { get; set; }

		// Token: 0x170006C2 RID: 1730
		// (get) Token: 0x06002531 RID: 9521 RVA: 0x00081A2C File Offset: 0x0007FC2C
		// (set) Token: 0x06002532 RID: 9522 RVA: 0x00081A34 File Offset: 0x0007FC34
		public int CurrentPackMode { get; set; } = -1;

		// Token: 0x170006C3 RID: 1731
		// (get) Token: 0x06002533 RID: 9523 RVA: 0x00081A40 File Offset: 0x0007FC40
		// (set) Token: 0x06002534 RID: 9524 RVA: 0x00081A48 File Offset: 0x0007FC48
		public byte BitOffset { get; set; } = byte.MaxValue;

		// Token: 0x170006C4 RID: 1732
		// (get) Token: 0x06002535 RID: 9525 RVA: 0x00081A54 File Offset: 0x0007FC54
		// (set) Token: 0x06002536 RID: 9526 RVA: 0x00081A5C File Offset: 0x0007FC5C
		public int SignatureId { get; set; } = Helper.InvalidId;

		// Token: 0x170006C5 RID: 1733
		// (get) Token: 0x06002537 RID: 9527 RVA: 0x00081A68 File Offset: 0x0007FC68
		// (set) Token: 0x06002538 RID: 9528 RVA: 0x00081A70 File Offset: 0x0007FC70
		public bool VirtualFunctionCall { get; set; }

		// Token: 0x170006C6 RID: 1734
		// (get) Token: 0x06002539 RID: 9529 RVA: 0x00081A7C File Offset: 0x0007FC7C
		// (set) Token: 0x0600253A RID: 9530 RVA: 0x00081A84 File Offset: 0x0007FC84
		public \u0011 IndexInfo { get; set; }

		// Token: 0x170006C7 RID: 1735
		// (get) Token: 0x0600253B RID: 9531 RVA: 0x00081A90 File Offset: 0x0007FC90
		// (set) Token: 0x0600253C RID: 9532 RVA: 0x00081A98 File Offset: 0x0007FC98
		public int CurrentScratchOffset { get; set; }

		// Token: 0x0400069F RID: 1695
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040006A0 RID: 1696
		[CompilerGenerated]
		private \u001D.\u0007 \u0001;

		// Token: 0x040006A1 RID: 1697
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040006A2 RID: 1698
		[CompilerGenerated]
		private IExpression \u0001;

		// Token: 0x040006A3 RID: 1699
		[CompilerGenerated]
		private AccessModeFlags \u0001;

		// Token: 0x040006A4 RID: 1700
		[CompilerGenerated]
		private ICompiledType \u0001;

		// Token: 0x040006A5 RID: 1701
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x040006A6 RID: 1702
		[CompilerGenerated]
		private byte \u0001;

		// Token: 0x040006A7 RID: 1703
		[CompilerGenerated]
		private int \u0003;

		// Token: 0x040006A8 RID: 1704
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x040006A9 RID: 1705
		[CompilerGenerated]
		private \u0011 \u0001;

		// Token: 0x040006AA RID: 1706
		[CompilerGenerated]
		private int \u0004;
	}
}
