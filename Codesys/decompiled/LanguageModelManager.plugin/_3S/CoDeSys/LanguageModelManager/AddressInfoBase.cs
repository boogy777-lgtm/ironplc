using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E6 RID: 230
	internal abstract class AddressInfoBase : IAddressInfo4, IAddressInfo3, IAddressInfo2, IAddressInfo
	{
		// Token: 0x0600113B RID: 4411 RVA: 0x0003207B File Offset: 0x0003107B
		protected AddressInfoBase(IType t)
		{
			this._iVariableID = -1;
			this._iSignatureID = -1;
			this._type = t;
		}

		// Token: 0x170004DB RID: 1243
		// (get) Token: 0x0600113C RID: 4412 RVA: 0x00004E6B File Offset: 0x00003E6B
		public virtual bool ContainsStackRelativeAddress
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004DC RID: 1244
		// (get) Token: 0x0600113D RID: 4413 RVA: 0x00032098 File Offset: 0x00031098
		public IType Type
		{
			get
			{
				return this._type;
			}
		}

		// Token: 0x170004DD RID: 1245
		// (get) Token: 0x0600113E RID: 4414 RVA: 0x000320A0 File Offset: 0x000310A0
		// (set) Token: 0x0600113F RID: 4415 RVA: 0x000320A8 File Offset: 0x000310A8
		public int VariableID
		{
			get
			{
				return this._iVariableID;
			}
			set
			{
				this._iVariableID = value;
			}
		}

		// Token: 0x170004DE RID: 1246
		// (get) Token: 0x06001140 RID: 4416 RVA: 0x000320B1 File Offset: 0x000310B1
		// (set) Token: 0x06001141 RID: 4417 RVA: 0x000320B9 File Offset: 0x000310B9
		public int SignatureID
		{
			get
			{
				return this._iSignatureID;
			}
			set
			{
				this._iSignatureID = value;
			}
		}

		// Token: 0x170004DF RID: 1247
		// (get) Token: 0x06001142 RID: 4418
		public abstract int Size { get; }

		// Token: 0x040003FF RID: 1023
		private int _iVariableID;

		// Token: 0x04000400 RID: 1024
		private int _iSignatureID;

		// Token: 0x04000401 RID: 1025
		private readonly IType _type;
	}
}
