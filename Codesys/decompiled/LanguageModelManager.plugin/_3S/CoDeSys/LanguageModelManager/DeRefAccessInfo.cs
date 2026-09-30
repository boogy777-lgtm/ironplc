using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E8 RID: 232
	internal class DeRefAccessInfo : AddressInfoBase, IDeRefAccessInfo3, IDeRefAccessInfo2, IDeRefAccessInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x06001153 RID: 4435 RVA: 0x000322AD File Offset: 0x000312AD
		public DeRefAccessInfo(IAddressInfo aiBase, int nSize, int nOffset, IType t) : base(t)
		{
			this.Base = (IMyAddressInfo)aiBase;
			this._baseSize = this.Base.Size;
			this.Offset = nOffset;
			this.Base.SetSize(nSize);
		}

		// Token: 0x170004E9 RID: 1257
		// (get) Token: 0x06001154 RID: 4436 RVA: 0x000322E7 File Offset: 0x000312E7
		// (set) Token: 0x06001155 RID: 4437 RVA: 0x000322EF File Offset: 0x000312EF
		public IMyAddressInfo Base { get; set; }

		// Token: 0x170004EA RID: 1258
		// (get) Token: 0x06001156 RID: 4438 RVA: 0x000322F8 File Offset: 0x000312F8
		IAddressInfo IDeRefAccessInfo.Base
		{
			get
			{
				return this.Base;
			}
		}

		// Token: 0x170004EB RID: 1259
		// (get) Token: 0x06001157 RID: 4439 RVA: 0x00032300 File Offset: 0x00031300
		public override int Size
		{
			get
			{
				return this.Base.Size;
			}
		}

		// Token: 0x06001158 RID: 4440 RVA: 0x0003230D File Offset: 0x0003130D
		public void SetSize(int nSize)
		{
			this.Base.SetSize(nSize);
		}

		// Token: 0x170004EC RID: 1260
		// (get) Token: 0x06001159 RID: 4441 RVA: 0x0003231B File Offset: 0x0003131B
		public int Offset { get; }

		// Token: 0x0600115A RID: 4442 RVA: 0x00032323 File Offset: 0x00031323
		public override bool Equals(object obj)
		{
			return this.Equals(obj as IDeRefAccessInfo);
		}

		// Token: 0x0600115B RID: 4443 RVA: 0x00032331 File Offset: 0x00031331
		public bool Equals(IDeRefAccessInfo other)
		{
			return other != null && other.Base.Equals(this.Base);
		}

		// Token: 0x0600115C RID: 4444 RVA: 0x0003234C File Offset: 0x0003134C
		public override int GetHashCode()
		{
			return this.Offset.GetHashCode() ^ this.Base.GetHashCode();
		}

		// Token: 0x0600115D RID: 4445 RVA: 0x00032373 File Offset: 0x00031373
		public IMyAddressInfo Duplicate()
		{
			return new DeRefAccessInfo(this.GetSizeAdjustedBase(), this.Size, this.Offset, base.Type)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x0600115E RID: 4446 RVA: 0x000323AA File Offset: 0x000313AA
		public IAddressInfo4 GetSizeAdjustedBase()
		{
			IMyAddressInfo myAddressInfo = this.Base.Duplicate();
			myAddressInfo.SetSize(this._baseSize);
			return myAddressInfo;
		}

		// Token: 0x170004ED RID: 1261
		// (get) Token: 0x0600115F RID: 4447 RVA: 0x000323C3 File Offset: 0x000313C3
		public override bool ContainsStackRelativeAddress
		{
			get
			{
				return this.Base.ContainsStackRelativeAddress;
			}
		}

		// Token: 0x0400040A RID: 1034
		private readonly int _baseSize;
	}
}
