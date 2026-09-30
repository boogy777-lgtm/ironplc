using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000EE RID: 238
	internal class PropertyAddressInfoExtended : AddressInfoBase, IPropertyAddressInfoExtended, IAddressInfo, IMyPropertyAddressInfo, IPropertyAdressInfoWithOffset, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x060011B0 RID: 4528 RVA: 0x0003299D File Offset: 0x0003199D
		public PropertyAddressInfoExtended(int nSize, IType t) : base(t)
		{
			this._nSize = nSize;
			this.OffsetValueGetter = -1;
			this.OffsetValueSetter = -1;
		}

		// Token: 0x17000512 RID: 1298
		// (get) Token: 0x060011B1 RID: 4529 RVA: 0x000329C9 File Offset: 0x000319C9
		public override int Size
		{
			get
			{
				return this._nSize;
			}
		}

		// Token: 0x060011B2 RID: 4530 RVA: 0x000329D1 File Offset: 0x000319D1
		public void SetSize(int nSize)
		{
			this._nSize = nSize;
		}

		// Token: 0x17000513 RID: 1299
		// (get) Token: 0x060011B3 RID: 4531 RVA: 0x000327A0 File Offset: 0x000317A0
		// (set) Token: 0x060011B4 RID: 4532 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public int AreaProperty
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
			set
			{
			}
		}

		// Token: 0x17000514 RID: 1300
		// (get) Token: 0x060011B5 RID: 4533 RVA: 0x000329DA File Offset: 0x000319DA
		// (set) Token: 0x060011B6 RID: 4534 RVA: 0x000329E2 File Offset: 0x000319E2
		public bool InterfaceCall
		{
			get
			{
				return this._bInterfaceCall;
			}
			set
			{
				this._bInterfaceCall = value;
			}
		}

		// Token: 0x17000515 RID: 1301
		// (get) Token: 0x060011B7 RID: 4535 RVA: 0x000327A0 File Offset: 0x000317A0
		// (set) Token: 0x060011B8 RID: 4536 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public int OffsetGet
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
			set
			{
			}
		}

		// Token: 0x17000516 RID: 1302
		// (get) Token: 0x060011B9 RID: 4537 RVA: 0x000327A0 File Offset: 0x000317A0
		// (set) Token: 0x060011BA RID: 4538 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public int OffsetSet
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
			set
			{
			}
		}

		// Token: 0x17000517 RID: 1303
		// (get) Token: 0x060011BB RID: 4539 RVA: 0x000329EB File Offset: 0x000319EB
		// (set) Token: 0x060011BC RID: 4540 RVA: 0x000329F3 File Offset: 0x000319F3
		public int VFTableOffsetGet
		{
			get
			{
				return this._nOffsetGet;
			}
			set
			{
				this._nOffsetGet = value;
			}
		}

		// Token: 0x17000518 RID: 1304
		// (get) Token: 0x060011BD RID: 4541 RVA: 0x000329FC File Offset: 0x000319FC
		// (set) Token: 0x060011BE RID: 4542 RVA: 0x00032A04 File Offset: 0x00031A04
		public int VFTableOffsetSet
		{
			get
			{
				return this._nOffsetSet;
			}
			set
			{
				this._nOffsetSet = value;
			}
		}

		// Token: 0x17000519 RID: 1305
		// (get) Token: 0x060011BF RID: 4543 RVA: 0x00032A0D File Offset: 0x00031A0D
		// (set) Token: 0x060011C0 RID: 4544 RVA: 0x00032A15 File Offset: 0x00031A15
		public IAddressInfo InfoInstance
		{
			get
			{
				return this._adrinfoInstance;
			}
			set
			{
				this._adrinfoInstance = value;
			}
		}

		// Token: 0x1700051A RID: 1306
		// (get) Token: 0x060011C1 RID: 4545 RVA: 0x00032A1E File Offset: 0x00031A1E
		// (set) Token: 0x060011C2 RID: 4546 RVA: 0x00032A26 File Offset: 0x00031A26
		public bool IsReferenceType { get; set; }

		// Token: 0x1700051B RID: 1307
		// (get) Token: 0x060011C3 RID: 4547 RVA: 0x00032A2F File Offset: 0x00031A2F
		// (set) Token: 0x060011C4 RID: 4548 RVA: 0x00032A37 File Offset: 0x00031A37
		public int OffsetValueGetter { get; set; }

		// Token: 0x1700051C RID: 1308
		// (get) Token: 0x060011C5 RID: 4549 RVA: 0x00032A40 File Offset: 0x00031A40
		// (set) Token: 0x060011C6 RID: 4550 RVA: 0x00032A48 File Offset: 0x00031A48
		public int OffsetValueSetter { get; set; }

		// Token: 0x1700051D RID: 1309
		// (get) Token: 0x060011C7 RID: 4551 RVA: 0x00032A51 File Offset: 0x00031A51
		public override bool ContainsStackRelativeAddress
		{
			get
			{
				return this._adrinfoInstance is IAddressInfo4 && (this._adrinfoInstance as IAddressInfo4).ContainsStackRelativeAddress;
			}
		}

		// Token: 0x060011C8 RID: 4552 RVA: 0x00032A78 File Offset: 0x00031A78
		public IMyAddressInfo Duplicate()
		{
			return new PropertyAddressInfoExtended(this.Size, base.Type)
			{
				InfoInstance = (this.InfoInstance as IMyAddressInfo).Duplicate(),
				VFTableOffsetGet = this.VFTableOffsetGet,
				VFTableOffsetSet = this.VFTableOffsetSet,
				InterfaceCall = this.InterfaceCall,
				SignatureID = base.SignatureID,
				VariableID = base.VariableID,
				IsReferenceType = this.IsReferenceType,
				OffsetValueGetter = this.OffsetValueGetter,
				OffsetValueSetter = this.OffsetValueSetter
			};
		}

		// Token: 0x060011C9 RID: 4553 RVA: 0x00032B0C File Offset: 0x00031B0C
		public override bool Equals(object obj)
		{
			IPropertyAddressInfoExtended propertyAddressInfoExtended = obj as IPropertyAddressInfoExtended;
			if (propertyAddressInfoExtended == null)
			{
				return false;
			}
			bool flag = propertyAddressInfoExtended.InfoInstance.Equals(this.InfoInstance) && propertyAddressInfoExtended.InterfaceCall == this.InterfaceCall && propertyAddressInfoExtended.VFTableOffsetGet == this.VFTableOffsetGet && propertyAddressInfoExtended.VFTableOffsetSet == this.VFTableOffsetSet && propertyAddressInfoExtended.Size == this.Size;
			if (flag && propertyAddressInfoExtended is IPropertyAddressInfoAdditional)
			{
				flag = (((IPropertyAddressInfoAdditional)propertyAddressInfoExtended).IsReferenceType == this.IsReferenceType);
			}
			return flag;
		}

		// Token: 0x060011CA RID: 4554 RVA: 0x00032B94 File Offset: 0x00031B94
		public override int GetHashCode()
		{
			return this.InfoInstance.GetHashCode() ^ this.InterfaceCall.GetHashCode() ^ this.VFTableOffsetSet.GetHashCode() ^ this.VFTableOffsetSet.GetHashCode() ^ this.Size.GetHashCode() ^ this.IsReferenceType.GetHashCode();
		}

		// Token: 0x04000420 RID: 1056
		private int _nSize;

		// Token: 0x04000421 RID: 1057
		private int _nOffsetGet = -1;

		// Token: 0x04000422 RID: 1058
		private int _nOffsetSet = -1;

		// Token: 0x04000423 RID: 1059
		private bool _bInterfaceCall;

		// Token: 0x04000424 RID: 1060
		private IAddressInfo _adrinfoInstance;
	}
}
