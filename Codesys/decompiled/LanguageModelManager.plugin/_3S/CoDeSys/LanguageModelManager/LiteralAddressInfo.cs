using System;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F7 RID: 247
	internal class LiteralAddressInfo : AddressInfoBase, ILiteralAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x06001228 RID: 4648 RVA: 0x00033656 File Offset: 0x00032656
		public LiteralAddressInfo(ILiteralValue lv, ICompiledType t) : base(t)
		{
			Debug.Assert(lv != null && t != null);
			this.m_lv = lv;
			this.m_t = t;
		}

		// Token: 0x17000544 RID: 1348
		// (get) Token: 0x06001229 RID: 4649 RVA: 0x0003367C File Offset: 0x0003267C
		public ILiteralValue Value
		{
			get
			{
				return this.m_lv;
			}
		}

		// Token: 0x17000545 RID: 1349
		// (get) Token: 0x0600122A RID: 4650 RVA: 0x00033684 File Offset: 0x00032684
		public override int Size
		{
			get
			{
				return this.m_t.Size(null);
			}
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void SetSize(int nSize)
		{
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x00033692 File Offset: 0x00032692
		public IMyAddressInfo Duplicate()
		{
			return new LiteralAddressInfo(this.m_lv, this.m_t)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x04000445 RID: 1093
		private readonly ILiteralValue m_lv;

		// Token: 0x04000446 RID: 1094
		private readonly ICompiledType m_t;
	}
}
