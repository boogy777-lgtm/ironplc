using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F2 RID: 242
	internal class SignedConstantAddressInfo : AddressInfoBase, ISignedConstantAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x060011E7 RID: 4583 RVA: 0x0003318E File Offset: 0x0003218E
		public SignedConstantAddressInfo(long lValue) : base(TypeTable.DInt)
		{
			this._lConstant = lValue;
		}

		// Token: 0x1700052A RID: 1322
		// (get) Token: 0x060011E8 RID: 4584 RVA: 0x000331A2 File Offset: 0x000321A2
		public override int Size
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x1700052B RID: 1323
		// (get) Token: 0x060011E9 RID: 4585 RVA: 0x000331A5 File Offset: 0x000321A5
		public long Value
		{
			get
			{
				return this._lConstant;
			}
		}

		// Token: 0x060011EA RID: 4586 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void SetSize(int nSize)
		{
		}

		// Token: 0x060011EB RID: 4587 RVA: 0x000331AD File Offset: 0x000321AD
		public IMyAddressInfo Duplicate()
		{
			return new SignedConstantAddressInfo(this._lConstant)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x060011EC RID: 4588 RVA: 0x000331D4 File Offset: 0x000321D4
		public override bool Equals(object obj)
		{
			SignedConstantAddressInfo signedConstantAddressInfo = obj as SignedConstantAddressInfo;
			return signedConstantAddressInfo != null && this._lConstant == signedConstantAddressInfo._lConstant;
		}

		// Token: 0x060011ED RID: 4589 RVA: 0x000331FC File Offset: 0x000321FC
		public override int GetHashCode()
		{
			return this._lConstant.GetHashCode();
		}

		// Token: 0x04000430 RID: 1072
		private readonly long _lConstant;
	}
}
