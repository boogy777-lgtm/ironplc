using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000F4 RID: 244
	internal class BitAddressInfo : AddressInfoBase, IBitAddressInfo2, IBitAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x060011FC RID: 4604 RVA: 0x00033318 File Offset: 0x00032318
		public BitAddressInfo(IAddressInfo aiBase, int nSize, int nBitOffset, IType t) : base(t)
		{
			this._aiBase = (aiBase as IMyAddressInfo);
			this._aiBase.SetSize(nSize);
			this._nBitOffset = nBitOffset;
		}

		// Token: 0x17000530 RID: 1328
		// (get) Token: 0x060011FD RID: 4605 RVA: 0x00033341 File Offset: 0x00032341
		public IAddressInfo Base
		{
			get
			{
				return this._aiBase;
			}
		}

		// Token: 0x17000531 RID: 1329
		// (get) Token: 0x060011FF RID: 4607 RVA: 0x00033352 File Offset: 0x00032352
		// (set) Token: 0x060011FE RID: 4606 RVA: 0x00033349 File Offset: 0x00032349
		public int BitOffset
		{
			get
			{
				return this._nBitOffset;
			}
			set
			{
				this._nBitOffset = value;
			}
		}

		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x06001200 RID: 4608 RVA: 0x0003335A File Offset: 0x0003235A
		public override int Size
		{
			get
			{
				return this._aiBase.Size;
			}
		}

		// Token: 0x06001201 RID: 4609 RVA: 0x00033367 File Offset: 0x00032367
		public void SetSize(int nSize)
		{
			this._aiBase.SetSize(nSize);
		}

		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x06001202 RID: 4610 RVA: 0x00033375 File Offset: 0x00032375
		// (set) Token: 0x06001203 RID: 4611 RVA: 0x0003337D File Offset: 0x0003237D
		public int AccessedElementSize { get; internal set; }

		// Token: 0x17000534 RID: 1332
		// (get) Token: 0x06001204 RID: 4612 RVA: 0x00033386 File Offset: 0x00032386
		public override bool ContainsStackRelativeAddress
		{
			get
			{
				return this._aiBase.ContainsStackRelativeAddress;
			}
		}

		// Token: 0x06001205 RID: 4613 RVA: 0x00033394 File Offset: 0x00032394
		public IMyAddressInfo Duplicate()
		{
			return new BitAddressInfo(this._aiBase.Duplicate(), this.Size, this._nBitOffset, base.Type)
			{
				AccessedElementSize = this.AccessedElementSize,
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x06001206 RID: 4614 RVA: 0x000333E8 File Offset: 0x000323E8
		public override bool Equals(object obj)
		{
			BitAddressInfo bitAddressInfo = obj as BitAddressInfo;
			return bitAddressInfo != null && (this._aiBase.Equals(bitAddressInfo._aiBase) && this._nBitOffset == bitAddressInfo._nBitOffset) && this.AccessedElementSize == bitAddressInfo.AccessedElementSize;
		}

		// Token: 0x06001207 RID: 4615 RVA: 0x00033434 File Offset: 0x00032434
		public override int GetHashCode()
		{
			return this._aiBase.GetHashCode() ^ this._nBitOffset.GetHashCode() ^ this.AccessedElementSize.GetHashCode();
		}

		// Token: 0x04000435 RID: 1077
		private readonly IMyAddressInfo _aiBase;

		// Token: 0x04000436 RID: 1078
		private int _nBitOffset;
	}
}
