using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000EB RID: 235
	internal class AbsoluteAddressInfo : AddressInfoBase, IAbsoluteAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x06001181 RID: 4481 RVA: 0x000325E4 File Offset: 0x000315E4
		public AbsoluteAddressInfo(int nArea, int nOffset, byte nBitOffset, int nSize, IType t) : base(t)
		{
			this._nArea = nArea;
			this._nOffset = nOffset;
			this._nBitOffset = nBitOffset;
			this._nSize = nSize;
		}

		// Token: 0x170004FD RID: 1277
		// (get) Token: 0x06001182 RID: 4482 RVA: 0x0003260B File Offset: 0x0003160B
		public override int Size
		{
			get
			{
				return this._nSize;
			}
		}

		// Token: 0x06001183 RID: 4483 RVA: 0x00032613 File Offset: 0x00031613
		public void SetSize(int nSize)
		{
			this._nSize = nSize;
		}

		// Token: 0x170004FE RID: 1278
		// (get) Token: 0x06001184 RID: 4484 RVA: 0x0003261C File Offset: 0x0003161C
		public int Area
		{
			get
			{
				return this._nArea;
			}
		}

		// Token: 0x170004FF RID: 1279
		// (get) Token: 0x06001185 RID: 4485 RVA: 0x00032624 File Offset: 0x00031624
		public int Offset
		{
			get
			{
				return this._nOffset;
			}
		}

		// Token: 0x17000500 RID: 1280
		// (get) Token: 0x06001186 RID: 4486 RVA: 0x0003262C File Offset: 0x0003162C
		public byte BitOffset
		{
			get
			{
				return this._nBitOffset;
			}
		}

		// Token: 0x06001187 RID: 4487 RVA: 0x00032634 File Offset: 0x00031634
		public override bool Equals(object obj)
		{
			IAbsoluteAddressInfo absoluteAddressInfo = obj as IAbsoluteAddressInfo;
			return absoluteAddressInfo != null && (absoluteAddressInfo.Area == this.Area && absoluteAddressInfo.Offset == this.Offset && absoluteAddressInfo.BitOffset == this.BitOffset) && absoluteAddressInfo.Size == this.Size;
		}

		// Token: 0x06001188 RID: 4488 RVA: 0x00032688 File Offset: 0x00031688
		public override int GetHashCode()
		{
			return this.Area.GetHashCode() ^ this.Offset.GetHashCode() ^ this.BitOffset.GetHashCode() ^ this.Size.GetHashCode();
		}

		// Token: 0x06001189 RID: 4489 RVA: 0x000326D0 File Offset: 0x000316D0
		public IMyAddressInfo Duplicate()
		{
			return new AbsoluteAddressInfo(this.Area, this.Offset, this.BitOffset, this.Size, base.Type)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID
			};
		}

		// Token: 0x04000413 RID: 1043
		private readonly int _nArea;

		// Token: 0x04000414 RID: 1044
		private readonly int _nOffset;

		// Token: 0x04000415 RID: 1045
		private readonly byte _nBitOffset;

		// Token: 0x04000416 RID: 1046
		private int _nSize;
	}
}
