using System;
using System.Runtime.CompilerServices;
using \u0011;
using \u0018;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0014
{
	// Token: 0x02000237 RID: 567
	internal sealed class \u000F : \u0018.\u0005, IExprInfo, IDeRefExprInfo
	{
		// Token: 0x170006D6 RID: 1750
		// (get) Token: 0x06002568 RID: 9576 RVA: 0x00081D30 File Offset: 0x0007FF30
		// (set) Token: 0x06002569 RID: 9577 RVA: 0x00081D38 File Offset: 0x0007FF38
		public int Offset { get; set; }

		// Token: 0x170006D7 RID: 1751
		// (get) Token: 0x0600256A RID: 9578 RVA: 0x00081D44 File Offset: 0x0007FF44
		// (set) Token: 0x0600256B RID: 9579 RVA: 0x00081D4C File Offset: 0x0007FF4C
		public bool InstanceAccess { get; set; }

		// Token: 0x170006D8 RID: 1752
		// (get) Token: 0x0600256C RID: 9580 RVA: 0x00081D58 File Offset: 0x0007FF58
		public IAccessMode AccessMode
		{
			get
			{
				return this._AccessMode;
			}
		}

		// Token: 0x170006D9 RID: 1753
		// (get) Token: 0x0600256D RID: 9581 RVA: 0x00081D60 File Offset: 0x0007FF60
		// (set) Token: 0x0600256E RID: 9582 RVA: 0x00081D68 File Offset: 0x0007FF68
		public global::\u0011.\u0008 _AccessMode { get; set; } = new global::\u0011.\u0008();

		// Token: 0x170006DA RID: 1754
		// (get) Token: 0x0600256F RID: 9583 RVA: 0x00081D74 File Offset: 0x0007FF74
		public bool IsBit
		{
			get
			{
				return this.BitNr != byte.MaxValue;
			}
		}

		// Token: 0x170006DB RID: 1755
		// (get) Token: 0x06002570 RID: 9584 RVA: 0x00081D88 File Offset: 0x0007FF88
		// (set) Token: 0x06002571 RID: 9585 RVA: 0x00081D90 File Offset: 0x0007FF90
		public byte BitNr { get; set; } = byte.MaxValue;

		// Token: 0x170006DC RID: 1756
		// (get) Token: 0x06002572 RID: 9586 RVA: 0x00081D9C File Offset: 0x0007FF9C
		// (set) Token: 0x06002573 RID: 9587 RVA: 0x00081DA4 File Offset: 0x0007FFA4
		public ICompiledType CompiledType { get; set; }

		// Token: 0x170006DD RID: 1757
		// (get) Token: 0x06002574 RID: 9588 RVA: 0x00081DB0 File Offset: 0x0007FFB0
		// (set) Token: 0x06002575 RID: 9589 RVA: 0x00081DB8 File Offset: 0x0007FFB8
		public IIndexInfo IndexInfo { get; set; }

		// Token: 0x170006DE RID: 1758
		// (get) Token: 0x06002576 RID: 9590 RVA: 0x00081DC4 File Offset: 0x0007FFC4
		// (set) Token: 0x06002577 RID: 9591 RVA: 0x00081DCC File Offset: 0x0007FFCC
		public int PackMode { get; set; } = -1;

		// Token: 0x040006B9 RID: 1721
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040006BA RID: 1722
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040006BB RID: 1723
		[CompilerGenerated]
		private global::\u0011.\u0008 \u0001;

		// Token: 0x040006BC RID: 1724
		[CompilerGenerated]
		private byte \u0001;

		// Token: 0x040006BD RID: 1725
		[CompilerGenerated]
		private ICompiledType \u0001;

		// Token: 0x040006BE RID: 1726
		[CompilerGenerated]
		private IIndexInfo \u0001;

		// Token: 0x040006BF RID: 1727
		[CompilerGenerated]
		private int \u0002;
	}
}
