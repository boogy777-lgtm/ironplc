using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E7 RID: 231
	internal class StackRelativeAddressInfo : AddressInfoBase, IStackRelativeAddressInfo3, IStackRelativeAddressInfo2, IStackRelativeAddressInfo, IAddressInfo, IMyAddressInfo, IAddressInfo4, IAddressInfo3, IAddressInfo2
	{
		// Token: 0x06001143 RID: 4419 RVA: 0x000320C2 File Offset: 0x000310C2
		public StackRelativeAddressInfo(int nOffset, byte nBitOffset, int nSize, int nAreaCode, int nOffsetCode, int nSizeCode, IType t) : base(t)
		{
			this._nOffset = nOffset;
			this._nBitOffset = nBitOffset;
			this._nSize = nSize;
			this._nAreaCode = nAreaCode;
			this._nOffsetCode = nOffsetCode;
			this._nSizeCode = nSizeCode;
		}

		// Token: 0x170004E0 RID: 1248
		// (get) Token: 0x06001144 RID: 4420 RVA: 0x000320F9 File Offset: 0x000310F9
		public int Offset
		{
			get
			{
				return this._nOffset;
			}
		}

		// Token: 0x170004E1 RID: 1249
		// (get) Token: 0x06001145 RID: 4421 RVA: 0x00032101 File Offset: 0x00031101
		// (set) Token: 0x06001146 RID: 4422 RVA: 0x00032109 File Offset: 0x00031109
		public bool StackRelative
		{
			get
			{
				return this._bStackRelative;
			}
			set
			{
				this._bStackRelative = value;
			}
		}

		// Token: 0x170004E2 RID: 1250
		// (get) Token: 0x06001147 RID: 4423 RVA: 0x00032112 File Offset: 0x00031112
		public byte BitOffset
		{
			get
			{
				return this._nBitOffset;
			}
		}

		// Token: 0x170004E3 RID: 1251
		// (get) Token: 0x06001148 RID: 4424 RVA: 0x0003211A File Offset: 0x0003111A
		public override int Size
		{
			get
			{
				return this._nSize;
			}
		}

		// Token: 0x06001149 RID: 4425 RVA: 0x00032122 File Offset: 0x00031122
		public void SetSize(int nSize)
		{
			this._nSize = nSize;
		}

		// Token: 0x170004E4 RID: 1252
		// (get) Token: 0x0600114A RID: 4426 RVA: 0x0003212B File Offset: 0x0003112B
		public int AreaCode
		{
			get
			{
				return this._nAreaCode;
			}
		}

		// Token: 0x170004E5 RID: 1253
		// (get) Token: 0x0600114B RID: 4427 RVA: 0x00032133 File Offset: 0x00031133
		public int OffsetCode
		{
			get
			{
				return this._nOffsetCode;
			}
		}

		// Token: 0x170004E6 RID: 1254
		// (get) Token: 0x0600114C RID: 4428 RVA: 0x0003213B File Offset: 0x0003113B
		public int SizeCode
		{
			get
			{
				return this._nSizeCode;
			}
		}

		// Token: 0x0600114D RID: 4429 RVA: 0x00032144 File Offset: 0x00031144
		public override bool Equals(object obj)
		{
			IStackRelativeAddressInfo2 stackRelativeAddressInfo = obj as IStackRelativeAddressInfo2;
			return stackRelativeAddressInfo != null && (stackRelativeAddressInfo.Offset == this.Offset && stackRelativeAddressInfo.BitOffset == this.BitOffset && stackRelativeAddressInfo.Size == this.Size && stackRelativeAddressInfo.AreaCode == this.AreaCode && stackRelativeAddressInfo.OffsetCode == this.OffsetCode && stackRelativeAddressInfo.SizeCode == this.SizeCode) && stackRelativeAddressInfo.StackRelative == this.StackRelative;
		}

		// Token: 0x0600114E RID: 4430 RVA: 0x000321C4 File Offset: 0x000311C4
		public override int GetHashCode()
		{
			return this.Offset.GetHashCode() ^ this.BitOffset.GetHashCode() ^ this.Size.GetHashCode() ^ this.AreaCode.GetHashCode() ^ this.OffsetCode.GetHashCode() ^ this.SizeCode.GetHashCode() ^ this.StackRelative.GetHashCode();
		}

		// Token: 0x0600114F RID: 4431 RVA: 0x0003223C File Offset: 0x0003123C
		public IMyAddressInfo Duplicate()
		{
			return new StackRelativeAddressInfo(this._nOffset, this._nBitOffset, this._nSize, this._nAreaCode, this._nOffsetCode, this._nSizeCode, base.Type)
			{
				SignatureID = base.SignatureID,
				VariableID = base.VariableID,
				_bStackRelative = this._bStackRelative
			};
		}

		// Token: 0x170004E7 RID: 1255
		// (get) Token: 0x06001150 RID: 4432 RVA: 0x0003229C File Offset: 0x0003129C
		// (set) Token: 0x06001151 RID: 4433 RVA: 0x000322A4 File Offset: 0x000312A4
		public ulong BasePointer
		{
			get
			{
				return this._basePointer;
			}
			set
			{
				this._basePointer = value;
			}
		}

		// Token: 0x170004E8 RID: 1256
		// (get) Token: 0x06001152 RID: 4434 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool ContainsStackRelativeAddress
		{
			get
			{
				return true;
			}
		}

		// Token: 0x04000402 RID: 1026
		private readonly int _nOffset;

		// Token: 0x04000403 RID: 1027
		private readonly byte _nBitOffset;

		// Token: 0x04000404 RID: 1028
		private int _nSize;

		// Token: 0x04000405 RID: 1029
		private readonly int _nAreaCode;

		// Token: 0x04000406 RID: 1030
		private readonly int _nOffsetCode;

		// Token: 0x04000407 RID: 1031
		private readonly int _nSizeCode;

		// Token: 0x04000408 RID: 1032
		private bool _bStackRelative;

		// Token: 0x04000409 RID: 1033
		private ulong _basePointer;
	}
}
