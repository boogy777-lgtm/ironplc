using System;
using System.Runtime.CompilerServices;
using \u0011;
using \u0018;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001D
{
	// Token: 0x0200023A RID: 570
	internal sealed class \u0007 : \u0018.\u0005, IExprInfo, IVariableExprInfo
	{
		// Token: 0x06002581 RID: 9601 RVA: 0x00081EC4 File Offset: 0x000800C4
		public static \u001D.\u0007 \u0001()
		{
			return new \u001D.\u0007();
		}

		// Token: 0x06002582 RID: 9602 RVA: 0x00081ECC File Offset: 0x000800CC
		public void \u0001()
		{
			this.VariableId = Helper.InvalidId;
			this.SignatureId = Helper.InvalidId;
			this.Area = 0;
			this.Address = 0;
			this.Offset = 0;
			this._AccessMode.\u0001();
			this.BitNr = byte.MaxValue;
			this.CompiledType = null;
			this.POUType = Operator.None;
			this.SignatureToCall = Helper.InvalidId;
			this.IndexInfo = null;
			this.PackMode = -1;
		}

		// Token: 0x170006E2 RID: 1762
		// (get) Token: 0x06002583 RID: 9603 RVA: 0x00081F44 File Offset: 0x00080144
		// (set) Token: 0x06002584 RID: 9604 RVA: 0x00081F4C File Offset: 0x0008014C
		public int Area { get; set; }

		// Token: 0x170006E3 RID: 1763
		// (get) Token: 0x06002585 RID: 9605 RVA: 0x00081F58 File Offset: 0x00080158
		// (set) Token: 0x06002586 RID: 9606 RVA: 0x00081F60 File Offset: 0x00080160
		public int Address { get; set; }

		// Token: 0x170006E4 RID: 1764
		// (get) Token: 0x06002587 RID: 9607 RVA: 0x00081F6C File Offset: 0x0008016C
		// (set) Token: 0x06002588 RID: 9608 RVA: 0x00081F74 File Offset: 0x00080174
		public int Offset { get; set; }

		// Token: 0x170006E5 RID: 1765
		// (get) Token: 0x06002589 RID: 9609 RVA: 0x00081F80 File Offset: 0x00080180
		public IAccessMode AccessMode
		{
			get
			{
				return this._AccessMode;
			}
		}

		// Token: 0x170006E6 RID: 1766
		// (get) Token: 0x0600258A RID: 9610 RVA: 0x00081F88 File Offset: 0x00080188
		// (set) Token: 0x0600258B RID: 9611 RVA: 0x00081F90 File Offset: 0x00080190
		public global::\u0011.\u0008 _AccessMode { get; set; } = new global::\u0011.\u0008();

		// Token: 0x0600258C RID: 9612 RVA: 0x00081F9C File Offset: 0x0008019C
		public bool \u0001(AccessModeFlags \u0002)
		{
			return this._AccessMode.\u0001(\u0002);
		}

		// Token: 0x0600258D RID: 9613 RVA: 0x00081FAC File Offset: 0x000801AC
		public bool \u0002(AccessModeFlags \u0002)
		{
			return this._AccessMode.\u0002(\u0002);
		}

		// Token: 0x0600258E RID: 9614 RVA: 0x00081FBC File Offset: 0x000801BC
		public void \u0001(AccessModeFlags \u0002, bool \u0003)
		{
			this._AccessMode.\u0001(\u0002, \u0003);
		}

		// Token: 0x170006E7 RID: 1767
		// (get) Token: 0x0600258F RID: 9615 RVA: 0x00081FCC File Offset: 0x000801CC
		// (set) Token: 0x06002590 RID: 9616 RVA: 0x00081FD4 File Offset: 0x000801D4
		public int SignatureToCall { get; set; } = Helper.InvalidId;

		// Token: 0x170006E8 RID: 1768
		// (get) Token: 0x06002591 RID: 9617 RVA: 0x00081FE0 File Offset: 0x000801E0
		public bool IsBit
		{
			get
			{
				return this.BitNr != byte.MaxValue;
			}
		}

		// Token: 0x170006E9 RID: 1769
		// (get) Token: 0x06002592 RID: 9618 RVA: 0x00081FF4 File Offset: 0x000801F4
		// (set) Token: 0x06002593 RID: 9619 RVA: 0x00081FFC File Offset: 0x000801FC
		public byte BitNr { get; set; } = byte.MaxValue;

		// Token: 0x170006EA RID: 1770
		// (get) Token: 0x06002594 RID: 9620 RVA: 0x00082008 File Offset: 0x00080208
		// (set) Token: 0x06002595 RID: 9621 RVA: 0x00082010 File Offset: 0x00080210
		public int VariableId { get; set; } = Helper.InvalidId;

		// Token: 0x170006EB RID: 1771
		// (get) Token: 0x06002596 RID: 9622 RVA: 0x0008201C File Offset: 0x0008021C
		// (set) Token: 0x06002597 RID: 9623 RVA: 0x00082024 File Offset: 0x00080224
		public int SignatureId { get; set; } = Helper.InvalidId;

		// Token: 0x170006EC RID: 1772
		// (get) Token: 0x06002598 RID: 9624 RVA: 0x00082030 File Offset: 0x00080230
		// (set) Token: 0x06002599 RID: 9625 RVA: 0x00082038 File Offset: 0x00080238
		public ICompiledType CompiledType { get; set; }

		// Token: 0x170006ED RID: 1773
		// (get) Token: 0x0600259A RID: 9626 RVA: 0x00082044 File Offset: 0x00080244
		// (set) Token: 0x0600259B RID: 9627 RVA: 0x0008204C File Offset: 0x0008024C
		public IIndexInfo IndexInfo { get; set; }

		// Token: 0x170006EE RID: 1774
		// (get) Token: 0x0600259C RID: 9628 RVA: 0x00082058 File Offset: 0x00080258
		// (set) Token: 0x0600259D RID: 9629 RVA: 0x00082060 File Offset: 0x00080260
		public Operator POUType { get; set; }

		// Token: 0x170006EF RID: 1775
		// (get) Token: 0x0600259E RID: 9630 RVA: 0x0008206C File Offset: 0x0008026C
		// (set) Token: 0x0600259F RID: 9631 RVA: 0x00082074 File Offset: 0x00080274
		public int PackMode { get; set; } = -1;

		// Token: 0x040006C3 RID: 1731
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040006C4 RID: 1732
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x040006C5 RID: 1733
		[CompilerGenerated]
		private int \u0003;

		// Token: 0x040006C6 RID: 1734
		[CompilerGenerated]
		private global::\u0011.\u0008 \u0001;

		// Token: 0x040006C7 RID: 1735
		[CompilerGenerated]
		private int \u0004;

		// Token: 0x040006C8 RID: 1736
		[CompilerGenerated]
		private byte \u0001;

		// Token: 0x040006C9 RID: 1737
		[CompilerGenerated]
		private int \u0005;

		// Token: 0x040006CA RID: 1738
		[CompilerGenerated]
		private int \u0006;

		// Token: 0x040006CB RID: 1739
		[CompilerGenerated]
		private ICompiledType \u0001;

		// Token: 0x040006CC RID: 1740
		[CompilerGenerated]
		private IIndexInfo \u0001;

		// Token: 0x040006CD RID: 1741
		[CompilerGenerated]
		private Operator \u0001;

		// Token: 0x040006CE RID: 1742
		[CompilerGenerated]
		private int \u0007;
	}
}
