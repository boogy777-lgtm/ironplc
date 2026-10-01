using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F3 RID: 243
	internal class AddressAddressInfo : AddressInfoBase, IAddressAddressInfo2, IAddressAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x060011EE RID: 4590 RVA: 0x00033217 File Offset: 0x00032217
		public AddressAddressInfo(ulong ulAddress) : base(TypeTable.DInt)
		{
			this.SetAddressAndSize(ulAddress);
		}

		// Token: 0x060011EF RID: 4591 RVA: 0x0003322B File Offset: 0x0003222B
		public AddressAddressInfo(int iArea, int iOffset) : base(TypeTable.DInt)
		{
			this.Area = iArea;
			this.Offset = iOffset;
		}

		// Token: 0x060011F0 RID: 4592 RVA: 0x00033246 File Offset: 0x00032246
		private void SetAddressAndSize(ulong ulAddress)
		{
			this._ulAddress = ulAddress;
			if ((ulAddress & 18446744069414584320UL) == 0UL)
			{
				this._iSize = 4;
				return;
			}
			this._iSize = 8;
		}

		// Token: 0x1700052C RID: 1324
		// (get) Token: 0x060011F1 RID: 4593 RVA: 0x0003326B File Offset: 0x0003226B
		public override int Size
		{
			get
			{
				return this._iSize;
			}
		}

		// Token: 0x1700052D RID: 1325
		// (get) Token: 0x060011F2 RID: 4594 RVA: 0x00033273 File Offset: 0x00032273
		public ulong Address
		{
			get
			{
				return this._ulAddress;
			}
		}

		// Token: 0x1700052E RID: 1326
		// (get) Token: 0x060011F3 RID: 4595 RVA: 0x0003327B File Offset: 0x0003227B
		// (set) Token: 0x060011F4 RID: 4596 RVA: 0x00033283 File Offset: 0x00032283
		public int Area { get; private set; }

		// Token: 0x1700052F RID: 1327
		// (get) Token: 0x060011F5 RID: 4597 RVA: 0x0003328C File Offset: 0x0003228C
		// (set) Token: 0x060011F6 RID: 4598 RVA: 0x00033294 File Offset: 0x00032294
		public int Offset { get; private set; }

		// Token: 0x060011F7 RID: 4599 RVA: 0x0003329D File Offset: 0x0003229D
		public void ConvertToAbsoluteAddress(ulong ulAreaStartAddress)
		{
			this.SetAddressAndSize(ulAreaStartAddress + (ulong)((long)this.Offset));
			this.Area = 0;
			this.Offset = 0;
		}

		// Token: 0x060011F8 RID: 4600 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void SetSize(int nSize)
		{
		}

		// Token: 0x060011F9 RID: 4601 RVA: 0x000332BC File Offset: 0x000322BC
		public IMyAddressInfo Duplicate()
		{
			return new AddressAddressInfo(this._ulAddress)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x060011FA RID: 4602 RVA: 0x000332E4 File Offset: 0x000322E4
		public override bool Equals(object obj)
		{
			AddressAddressInfo addressAddressInfo = obj as AddressAddressInfo;
			return addressAddressInfo != null && this._ulAddress == addressAddressInfo._ulAddress;
		}

		// Token: 0x060011FB RID: 4603 RVA: 0x0003330B File Offset: 0x0003230B
		[SuppressMessage("Minor Bug", "S2328:\"GetHashCode\" should not reference mutable fields", Justification = "The address member is in fact not mutable after method ConvertToAbsoluteAddress has been called")]
		public override int GetHashCode()
		{
			return this._ulAddress.GetHashCode();
		}

		// Token: 0x04000431 RID: 1073
		private ulong _ulAddress;

		// Token: 0x04000432 RID: 1074
		private int _iSize;
	}
}
