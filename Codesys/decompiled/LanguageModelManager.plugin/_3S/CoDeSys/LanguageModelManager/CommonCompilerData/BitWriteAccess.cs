using System;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C4 RID: 452
	[TypeGuid("{21D65E57-B4FD-47C4-A391-9E68ADF2CFFE}")]
	[StorageVersion("3.5.3.0")]
	public class BitWriteAccess : GenericObject2, IBitWriteAccess, IBitWriteAccessSerializable
	{
		// Token: 0x17000851 RID: 2129
		// (get) Token: 0x06001FF2 RID: 8178 RVA: 0x00058A8C File Offset: 0x00057A8C
		// (set) Token: 0x06001FF3 RID: 8179 RVA: 0x00058AD8 File Offset: 0x00057AD8
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Pos")]
		[StorageVersion("3.5.3.0")]
		public long PositionCombination
		{
			get
			{
				if (this.Position == null)
				{
					return -1L;
				}
				return new IntegerUnion
				{
					m_long = this.Position.EditorPosition,
					m_short3 = this.Position.PositionOffset
				}.m_long;
			}
			set
			{
				if (value == -1L)
				{
					this.Position = null;
					return;
				}
				long nPosition;
				short sPositionOffset;
				PositionHelper.SplitPosition(value, ref nPosition, ref sPositionOffset);
				this.Position = MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
			}
		}

		// Token: 0x06001FF4 RID: 8180 RVA: 0x00058B09 File Offset: 0x00057B09
		public BitWriteAccess()
		{
		}

		// Token: 0x06001FF5 RID: 8181 RVA: 0x00058B3C File Offset: 0x00057B3C
		public BitWriteAccess(int nSignatureId, int nArea, int nOffset, byte byBitNr, IMinimalPosition position, string stSymbol)
		{
			if (byBitNr > 7)
			{
				throw new ArgumentException(string.Format("{0}({1}) must be in the range [0; 7].", "byBitNr", byBitNr), "byBitNr");
			}
			this._nSignatureId = nSignatureId;
			this._nArea = nArea;
			this._nOffset = nOffset;
			this._byBitNr = byBitNr;
			this.Position = position;
			this._stSymbol = stSymbol;
		}

		// Token: 0x17000852 RID: 2130
		// (get) Token: 0x06001FF6 RID: 8182 RVA: 0x00058BCD File Offset: 0x00057BCD
		public int SignatureId
		{
			get
			{
				return this._nSignatureId;
			}
		}

		// Token: 0x17000853 RID: 2131
		// (get) Token: 0x06001FF7 RID: 8183 RVA: 0x00058BD5 File Offset: 0x00057BD5
		// (set) Token: 0x06001FF8 RID: 8184 RVA: 0x00058BDD File Offset: 0x00057BDD
		public IMinimalPosition Position { get; private set; }

		// Token: 0x17000854 RID: 2132
		// (get) Token: 0x06001FF9 RID: 8185 RVA: 0x00058BE6 File Offset: 0x00057BE6
		public string Symbol
		{
			get
			{
				return this._stSymbol;
			}
		}

		// Token: 0x17000855 RID: 2133
		// (get) Token: 0x06001FFA RID: 8186 RVA: 0x00058BEE File Offset: 0x00057BEE
		public int Area
		{
			get
			{
				return this._nArea;
			}
		}

		// Token: 0x17000856 RID: 2134
		// (get) Token: 0x06001FFB RID: 8187 RVA: 0x00058BF6 File Offset: 0x00057BF6
		public int Offset
		{
			get
			{
				return this._nOffset;
			}
		}

		// Token: 0x17000857 RID: 2135
		// (get) Token: 0x06001FFC RID: 8188 RVA: 0x00058BFE File Offset: 0x00057BFE
		public byte BitNr
		{
			get
			{
				return this._byBitNr;
			}
		}

		// Token: 0x0400064A RID: 1610
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("SignId")]
		[StorageVersion("3.5.3.0")]
		private int _nSignatureId = -1;

		// Token: 0x0400064B RID: 1611
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Area")]
		[StorageVersion("3.5.3.0")]
		private int _nArea = -1;

		// Token: 0x0400064C RID: 1612
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Offset")]
		[StorageVersion("3.5.3.0")]
		private int _nOffset = -1;

		// Token: 0x0400064D RID: 1613
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("BirNr")]
		[StorageVersion("3.5.3.0")]
		private byte _byBitNr = byte.MaxValue;

		// Token: 0x0400064E RID: 1614
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Symbol")]
		[StorageVersion("3.5.3.0")]
		private string _stSymbol = string.Empty;
	}
}
