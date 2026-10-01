using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000ED RID: 237
	internal class PropertyAddressInfo : AddressInfoBase, IPropertyAddressInfo, IAddressInfo, IPropertyAddressInfoAdditional, IMyPropertyAddressInfo, IPropertyAdressInfoWithOffset, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x06001196 RID: 4502 RVA: 0x00032710 File Offset: 0x00031710
		public PropertyAddressInfo(int nSize, IType t) : base(t)
		{
			this._nSize = nSize;
			this.OffsetValueGetter = -1;
			this.OffsetValueSetter = -1;
		}

		// Token: 0x17000507 RID: 1287
		// (get) Token: 0x06001197 RID: 4503 RVA: 0x0003275C File Offset: 0x0003175C
		public override int Size
		{
			get
			{
				return this._nSize;
			}
		}

		// Token: 0x06001198 RID: 4504 RVA: 0x00032764 File Offset: 0x00031764
		public void SetSize(int nSize)
		{
			this._nSize = nSize;
		}

		// Token: 0x17000508 RID: 1288
		// (get) Token: 0x06001199 RID: 4505 RVA: 0x0003276D File Offset: 0x0003176D
		// (set) Token: 0x0600119A RID: 4506 RVA: 0x00032775 File Offset: 0x00031775
		public int AreaProperty
		{
			get
			{
				return this._nAreaProperty;
			}
			set
			{
				this._nAreaProperty = value;
			}
		}

		// Token: 0x17000509 RID: 1289
		// (get) Token: 0x0600119B RID: 4507 RVA: 0x0003277E File Offset: 0x0003177E
		// (set) Token: 0x0600119C RID: 4508 RVA: 0x00032786 File Offset: 0x00031786
		public int OffsetGet
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

		// Token: 0x1700050A RID: 1290
		// (get) Token: 0x0600119D RID: 4509 RVA: 0x0003278F File Offset: 0x0003178F
		// (set) Token: 0x0600119E RID: 4510 RVA: 0x00032797 File Offset: 0x00031797
		public int OffsetSet
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

		// Token: 0x1700050B RID: 1291
		// (get) Token: 0x0600119F RID: 4511 RVA: 0x000327A0 File Offset: 0x000317A0
		// (set) Token: 0x060011A0 RID: 4512 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public int VFTableOffsetGet
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
			set
			{
			}
		}

		// Token: 0x1700050C RID: 1292
		// (get) Token: 0x060011A1 RID: 4513 RVA: 0x000327A0 File Offset: 0x000317A0
		// (set) Token: 0x060011A2 RID: 4514 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public int VFTableOffsetSet
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
			set
			{
			}
		}

		// Token: 0x1700050D RID: 1293
		// (get) Token: 0x060011A3 RID: 4515 RVA: 0x000327A7 File Offset: 0x000317A7
		// (set) Token: 0x060011A4 RID: 4516 RVA: 0x000327AF File Offset: 0x000317AF
		public int AreaInstance
		{
			get
			{
				return this._nAreaInstance;
			}
			set
			{
				this._nAreaInstance = value;
			}
		}

		// Token: 0x1700050E RID: 1294
		// (get) Token: 0x060011A5 RID: 4517 RVA: 0x000327B8 File Offset: 0x000317B8
		// (set) Token: 0x060011A6 RID: 4518 RVA: 0x000327C0 File Offset: 0x000317C0
		public int OffsetInstance
		{
			get
			{
				return this._nOffsetInstance;
			}
			set
			{
				this._nOffsetInstance = value;
			}
		}

		// Token: 0x1700050F RID: 1295
		// (get) Token: 0x060011A7 RID: 4519 RVA: 0x000327C9 File Offset: 0x000317C9
		// (set) Token: 0x060011A8 RID: 4520 RVA: 0x000327D1 File Offset: 0x000317D1
		public bool IsReferenceType { get; set; }

		// Token: 0x17000510 RID: 1296
		// (get) Token: 0x060011A9 RID: 4521 RVA: 0x000327DA File Offset: 0x000317DA
		// (set) Token: 0x060011AA RID: 4522 RVA: 0x000327E2 File Offset: 0x000317E2
		public int OffsetValueGetter { get; set; }

		// Token: 0x17000511 RID: 1297
		// (get) Token: 0x060011AB RID: 4523 RVA: 0x000327EB File Offset: 0x000317EB
		// (set) Token: 0x060011AC RID: 4524 RVA: 0x000327F3 File Offset: 0x000317F3
		public int OffsetValueSetter { get; set; }

		// Token: 0x060011AD RID: 4525 RVA: 0x000327FC File Offset: 0x000317FC
		public IMyAddressInfo Duplicate()
		{
			return new PropertyAddressInfo(this.Size, base.Type)
			{
				AreaInstance = this.AreaInstance,
				AreaProperty = this.AreaProperty,
				OffsetGet = this.OffsetGet,
				OffsetSet = this.OffsetSet,
				OffsetInstance = this.OffsetInstance,
				SignatureID = base.SignatureID,
				VariableID = base.VariableID,
				IsReferenceType = this.IsReferenceType,
				OffsetValueGetter = this.OffsetValueGetter,
				OffsetValueSetter = this.OffsetValueSetter
			};
		}

		// Token: 0x060011AE RID: 4526 RVA: 0x00032894 File Offset: 0x00031894
		public override bool Equals(object obj)
		{
			IPropertyAddressInfo propertyAddressInfo = obj as IPropertyAddressInfo;
			if (propertyAddressInfo == null)
			{
				return false;
			}
			bool flag = propertyAddressInfo.AreaInstance == this.AreaInstance && propertyAddressInfo.AreaProperty == this.AreaProperty && propertyAddressInfo.OffsetGet == this.OffsetGet && propertyAddressInfo.OffsetSet == this.OffsetSet && propertyAddressInfo.OffsetInstance == this.OffsetInstance && propertyAddressInfo.Size == this.Size;
			if (flag && propertyAddressInfo is IPropertyAddressInfoAdditional)
			{
				flag = (((IPropertyAddressInfoAdditional)propertyAddressInfo).IsReferenceType == this.IsReferenceType);
			}
			return flag;
		}

		// Token: 0x060011AF RID: 4527 RVA: 0x00032928 File Offset: 0x00031928
		public override int GetHashCode()
		{
			return this.AreaInstance.GetHashCode() ^ this.AreaProperty.GetHashCode() ^ this.OffsetGet.GetHashCode() ^ this.OffsetSet.GetHashCode() ^ this.OffsetInstance.GetHashCode() ^ this.Size.GetHashCode() ^ this.IsReferenceType.GetHashCode();
		}

		// Token: 0x04000417 RID: 1047
		private int _nSize;

		// Token: 0x04000418 RID: 1048
		private int _nAreaProperty = -1;

		// Token: 0x04000419 RID: 1049
		private int _nOffsetGet = -1;

		// Token: 0x0400041A RID: 1050
		private int _nOffsetSet = -1;

		// Token: 0x0400041B RID: 1051
		private int _nAreaInstance = -1;

		// Token: 0x0400041C RID: 1052
		private int _nOffsetInstance = -1;
	}
}
