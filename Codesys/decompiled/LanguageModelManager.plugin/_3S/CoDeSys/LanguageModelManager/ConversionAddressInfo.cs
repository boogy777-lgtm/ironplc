using System;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F8 RID: 248
	internal class ConversionAddressInfo : AddressInfoBase, IConversionAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x0600122D RID: 4653 RVA: 0x000336BD File Offset: 0x000326BD
		public ConversionAddressInfo(TypeClass tcFrom, TypeClass tcTo, IAddressInfo aiBase, bool bImplicit, IType t) : base(t)
		{
			Debug.Assert(aiBase != null && t != null);
			this.m_tcFrom = tcFrom;
			this.m_tcTo = tcTo;
			this.m_aiBase = aiBase;
			this.m_bImplicit = bImplicit;
		}

		// Token: 0x17000546 RID: 1350
		// (get) Token: 0x0600122E RID: 4654 RVA: 0x000336F4 File Offset: 0x000326F4
		public TypeClass From
		{
			get
			{
				return this.m_tcFrom;
			}
		}

		// Token: 0x17000547 RID: 1351
		// (get) Token: 0x0600122F RID: 4655 RVA: 0x000336FC File Offset: 0x000326FC
		public TypeClass To
		{
			get
			{
				return this.m_tcTo;
			}
		}

		// Token: 0x17000548 RID: 1352
		// (get) Token: 0x06001230 RID: 4656 RVA: 0x00033704 File Offset: 0x00032704
		public IAddressInfo Base
		{
			get
			{
				return this.m_aiBase;
			}
		}

		// Token: 0x17000549 RID: 1353
		// (get) Token: 0x06001231 RID: 4657 RVA: 0x0003370C File Offset: 0x0003270C
		public bool Implicit
		{
			get
			{
				return this.m_bImplicit;
			}
		}

		// Token: 0x1700054A RID: 1354
		// (get) Token: 0x06001232 RID: 4658 RVA: 0x00033714 File Offset: 0x00032714
		public override int Size
		{
			get
			{
				return TypeTable.Get(base.Type.Class).Size(null);
			}
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void SetSize(int nSize)
		{
		}

		// Token: 0x06001234 RID: 4660 RVA: 0x0003372C File Offset: 0x0003272C
		public IMyAddressInfo Duplicate()
		{
			return new ConversionAddressInfo(this.m_tcFrom, this.m_tcTo, ((IMyAddressInfo)this.m_aiBase).Duplicate(), this.m_bImplicit, base.Type)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x1700054B RID: 1355
		// (get) Token: 0x06001235 RID: 4661 RVA: 0x0003377E File Offset: 0x0003277E
		public override bool ContainsStackRelativeAddress
		{
			get
			{
				return this.m_aiBase is IAddressInfo4 && (this.m_aiBase as IAddressInfo4).ContainsStackRelativeAddress;
			}
		}

		// Token: 0x04000447 RID: 1095
		private readonly TypeClass m_tcFrom;

		// Token: 0x04000448 RID: 1096
		private readonly TypeClass m_tcTo;

		// Token: 0x04000449 RID: 1097
		private readonly IAddressInfo m_aiBase;

		// Token: 0x0400044A RID: 1098
		private readonly bool m_bImplicit;
	}
}
