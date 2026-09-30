using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200012E RID: 302
	public class MemorySettings : _IMemorySettings2, _IMemorySettings, IMemorySettings3, IMemorySettings2, IMemorySettings
	{
		// Token: 0x06001A1F RID: 6687 RVA: 0x0004B284 File Offset: 0x0004A284
		public MemorySettings(bool p_bRetainInOwn)
		{
			this.m_bRetainInOwn = p_bRetainInOwn;
			this.m_iDataSegmentSize = MemorySettings.NoSegmentation;
			this.m_iCodeSegmentSize = MemorySettings.NoSegmentation;
			this.m_iCodeSegmentPrologSize = 0;
			this.m_iCodeSegmentHeaderSize = 0;
			this.m_GlobalDataSize = 0;
			this.m_CodeSize = 0;
			this.m_MemoryDataSize = 0;
			this.m_InputDataSize = 0;
			this.m_OutputDataSize = 0;
			this.m_RetainDataSize = 0;
			this.m_iMaxDataSize = MemorySettings.SizeNoLimit;
			this.m_iMaxCodeSize = MemorySettings.SizeNoLimit;
			this.m_iPackMode = 8;
			this.m_iMinSize = 0;
			this.m_iStackAlignment = 4;
			this.m_bByteAddressing = false;
			this.m_bBitByteAddressing = true;
			this.m_bBitWordAddressing = false;
			this.m_bOCInOwnSegment = false;
			this.m_alAreas = new LList<_IArea>();
			this.m_bAdditionalAreas = true;
			this.m_iMaxSizeForOnlineChange = int.MaxValue;
			this.m_bRetainDynamic = false;
			this.m_bPersistentDynamic = false;
			this.m_iMinGranularity = -1;
			this.m_iAreaAlignment = 8;
		}

		// Token: 0x170006D3 RID: 1747
		// (get) Token: 0x06001A20 RID: 6688 RVA: 0x000327A0 File Offset: 0x000317A0
		public static int SizeNoLimit
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
		}

		// Token: 0x170006D4 RID: 1748
		// (get) Token: 0x06001A21 RID: 6689 RVA: 0x000327A0 File Offset: 0x000317A0
		public static int NoSegmentation
		{
			get
			{
				return SignatureConstant.InvalidOffset;
			}
		}

		// Token: 0x170006D5 RID: 1749
		// (get) Token: 0x06001A22 RID: 6690 RVA: 0x0004B36C File Offset: 0x0004A36C
		public static ushort InvalidRefId
		{
			get
			{
				return ushort.MaxValue;
			}
		}

		// Token: 0x170006D6 RID: 1750
		// (get) Token: 0x06001A23 RID: 6691 RVA: 0x0004B373 File Offset: 0x0004A373
		// (set) Token: 0x06001A24 RID: 6692 RVA: 0x0004B37B File Offset: 0x0004A37B
		public int PackMode
		{
			get
			{
				return this.m_iPackMode;
			}
			set
			{
				this.m_iPackMode = ((value != 0) ? value : 1);
			}
		}

		// Token: 0x170006D7 RID: 1751
		// (get) Token: 0x06001A25 RID: 6693 RVA: 0x0004B38A File Offset: 0x0004A38A
		// (set) Token: 0x06001A26 RID: 6694 RVA: 0x0004B392 File Offset: 0x0004A392
		public int MinSize
		{
			get
			{
				return this.m_iMinSize;
			}
			set
			{
				this.m_iMinSize = value;
			}
		}

		// Token: 0x170006D8 RID: 1752
		// (get) Token: 0x06001A27 RID: 6695 RVA: 0x0004B39B File Offset: 0x0004A39B
		// (set) Token: 0x06001A28 RID: 6696 RVA: 0x0004B3A3 File Offset: 0x0004A3A3
		public int StackAlignment
		{
			get
			{
				return this.m_iStackAlignment;
			}
			set
			{
				this.m_iStackAlignment = value;
			}
		}

		// Token: 0x170006D9 RID: 1753
		// (get) Token: 0x06001A29 RID: 6697 RVA: 0x0004B3AC File Offset: 0x0004A3AC
		// (set) Token: 0x06001A2A RID: 6698 RVA: 0x0004B3B4 File Offset: 0x0004A3B4
		public bool ByteAddressing
		{
			get
			{
				return this.m_bByteAddressing;
			}
			set
			{
				this.m_bByteAddressing = value;
			}
		}

		// Token: 0x170006DA RID: 1754
		// (get) Token: 0x06001A2B RID: 6699 RVA: 0x0004B3BD File Offset: 0x0004A3BD
		// (set) Token: 0x06001A2C RID: 6700 RVA: 0x0004B3C5 File Offset: 0x0004A3C5
		public bool BitByteAddressing
		{
			get
			{
				return this.m_bBitByteAddressing;
			}
			set
			{
				this.m_bBitByteAddressing = value;
			}
		}

		// Token: 0x170006DB RID: 1755
		// (get) Token: 0x06001A2D RID: 6701 RVA: 0x0004B3CE File Offset: 0x0004A3CE
		// (set) Token: 0x06001A2E RID: 6702 RVA: 0x0004B3D6 File Offset: 0x0004A3D6
		public bool BitWordAddressing
		{
			get
			{
				return this.m_bBitWordAddressing;
			}
			set
			{
				this.m_bBitWordAddressing = value;
			}
		}

		// Token: 0x170006DC RID: 1756
		// (get) Token: 0x06001A2F RID: 6703 RVA: 0x0004B3DF File Offset: 0x0004A3DF
		// (set) Token: 0x06001A30 RID: 6704 RVA: 0x0004B3E7 File Offset: 0x0004A3E7
		public bool OnlineChangeInOwnSegment
		{
			get
			{
				return this.m_bOCInOwnSegment;
			}
			set
			{
				this.m_bOCInOwnSegment = value;
			}
		}

		// Token: 0x170006DD RID: 1757
		// (get) Token: 0x06001A31 RID: 6705 RVA: 0x0004B3F0 File Offset: 0x0004A3F0
		// (set) Token: 0x06001A32 RID: 6706 RVA: 0x0004B3F8 File Offset: 0x0004A3F8
		public bool AdditionalAreas
		{
			get
			{
				return this.m_bAdditionalAreas;
			}
			set
			{
				this.m_bAdditionalAreas = value;
			}
		}

		// Token: 0x170006DE RID: 1758
		// (get) Token: 0x06001A33 RID: 6707 RVA: 0x0004B401 File Offset: 0x0004A401
		// (set) Token: 0x06001A34 RID: 6708 RVA: 0x0004B409 File Offset: 0x0004A409
		public bool OneSRAM
		{
			get
			{
				return this._bOneSRAMArea;
			}
			set
			{
				this._bOneSRAMArea = value;
			}
		}

		// Token: 0x170006DF RID: 1759
		// (get) Token: 0x06001A35 RID: 6709 RVA: 0x0004B412 File Offset: 0x0004A412
		// (set) Token: 0x06001A36 RID: 6710 RVA: 0x0004B41A File Offset: 0x0004A41A
		public bool LateRelocationForFixedAreas { get; set; }

		// Token: 0x170006E0 RID: 1760
		// (get) Token: 0x06001A37 RID: 6711 RVA: 0x0004B423 File Offset: 0x0004A423
		public static MemorySettings Empty
		{
			get
			{
				return new MemorySettings(false);
			}
		}

		// Token: 0x170006E1 RID: 1761
		// (get) Token: 0x06001A38 RID: 6712 RVA: 0x0004B42B File Offset: 0x0004A42B
		// (set) Token: 0x06001A39 RID: 6713 RVA: 0x0004B433 File Offset: 0x0004A433
		public bool RetainInOwnSegment
		{
			get
			{
				return this.m_bRetainInOwn;
			}
			set
			{
				this.m_bRetainInOwn = value;
			}
		}

		// Token: 0x170006E2 RID: 1762
		// (get) Token: 0x06001A3A RID: 6714 RVA: 0x0004B43C File Offset: 0x0004A43C
		// (set) Token: 0x06001A3B RID: 6715 RVA: 0x0004B444 File Offset: 0x0004A444
		public bool RetainDynamic
		{
			get
			{
				return this.m_bRetainDynamic;
			}
			set
			{
				this.m_bRetainDynamic = value;
			}
		}

		// Token: 0x170006E3 RID: 1763
		// (get) Token: 0x06001A3C RID: 6716 RVA: 0x0004B44D File Offset: 0x0004A44D
		// (set) Token: 0x06001A3D RID: 6717 RVA: 0x0004B455 File Offset: 0x0004A455
		public bool PersistentDynamic
		{
			get
			{
				return this.m_bPersistentDynamic;
			}
			set
			{
				this.m_bPersistentDynamic = value;
			}
		}

		// Token: 0x170006E4 RID: 1764
		// (get) Token: 0x06001A3E RID: 6718 RVA: 0x0004B45E File Offset: 0x0004A45E
		// (set) Token: 0x06001A3F RID: 6719 RVA: 0x0004B466 File Offset: 0x0004A466
		public int DataSegmentSize
		{
			get
			{
				return this.m_iDataSegmentSize;
			}
			set
			{
				this.m_iDataSegmentSize = value;
			}
		}

		// Token: 0x170006E5 RID: 1765
		// (get) Token: 0x06001A40 RID: 6720 RVA: 0x0004B46F File Offset: 0x0004A46F
		// (set) Token: 0x06001A41 RID: 6721 RVA: 0x0004B477 File Offset: 0x0004A477
		public int CodeSegmentSize
		{
			get
			{
				return this.m_iCodeSegmentSize;
			}
			set
			{
				this.m_iCodeSegmentSize = value;
			}
		}

		// Token: 0x170006E6 RID: 1766
		// (get) Token: 0x06001A42 RID: 6722 RVA: 0x0004B480 File Offset: 0x0004A480
		// (set) Token: 0x06001A43 RID: 6723 RVA: 0x0004B488 File Offset: 0x0004A488
		public int CodeSegmentPrologSize
		{
			get
			{
				return this.m_iCodeSegmentPrologSize;
			}
			set
			{
				this.m_iCodeSegmentPrologSize = value;
			}
		}

		// Token: 0x170006E7 RID: 1767
		// (get) Token: 0x06001A44 RID: 6724 RVA: 0x0004B491 File Offset: 0x0004A491
		// (set) Token: 0x06001A45 RID: 6725 RVA: 0x0004B499 File Offset: 0x0004A499
		public int CodeSegmentHeaderSize
		{
			get
			{
				return this.m_iCodeSegmentHeaderSize;
			}
			set
			{
				this.m_iCodeSegmentHeaderSize = value;
			}
		}

		// Token: 0x170006E8 RID: 1768
		// (get) Token: 0x06001A46 RID: 6726 RVA: 0x0004B4A2 File Offset: 0x0004A4A2
		// (set) Token: 0x06001A47 RID: 6727 RVA: 0x0004B4AA File Offset: 0x0004A4AA
		public int MaxSizeForOnlineChange
		{
			get
			{
				return this.m_iMaxSizeForOnlineChange;
			}
			set
			{
				this.m_iMaxSizeForOnlineChange = value;
			}
		}

		// Token: 0x170006E9 RID: 1769
		// (get) Token: 0x06001A48 RID: 6728 RVA: 0x0004B4B3 File Offset: 0x0004A4B3
		// (set) Token: 0x06001A49 RID: 6729 RVA: 0x0004B4BB File Offset: 0x0004A4BB
		public int MinGranularity
		{
			get
			{
				return this.m_iMinGranularity;
			}
			set
			{
				this.m_iMinGranularity = value;
			}
		}

		// Token: 0x170006EA RID: 1770
		// (get) Token: 0x06001A4A RID: 6730 RVA: 0x0004B4C4 File Offset: 0x0004A4C4
		// (set) Token: 0x06001A4B RID: 6731 RVA: 0x0004B4CC File Offset: 0x0004A4CC
		public int MaxDataSize
		{
			get
			{
				return this.m_iMaxDataSize;
			}
			set
			{
				this.m_iMaxDataSize = value;
			}
		}

		// Token: 0x170006EB RID: 1771
		// (get) Token: 0x06001A4C RID: 6732 RVA: 0x0004B4D5 File Offset: 0x0004A4D5
		// (set) Token: 0x06001A4D RID: 6733 RVA: 0x0004B4DD File Offset: 0x0004A4DD
		public int MaxCodeSize
		{
			get
			{
				return this.m_iMaxCodeSize;
			}
			set
			{
				this.m_iMaxCodeSize = value;
			}
		}

		// Token: 0x06001A4E RID: 6734 RVA: 0x0004B4E6 File Offset: 0x0004A4E6
		public void AddArea(IArea area)
		{
			this.m_alAreas.Add(area as _IArea);
		}

		// Token: 0x170006EC RID: 1772
		// (get) Token: 0x06001A4F RID: 6735 RVA: 0x0004B4F9 File Offset: 0x0004A4F9
		public IList<_IArea> AreaList
		{
			get
			{
				return this.m_alAreas.AsReadOnly();
			}
		}

		// Token: 0x170006ED RID: 1773
		// (get) Token: 0x06001A50 RID: 6736 RVA: 0x0004B508 File Offset: 0x0004A508
		public IArea[] Areas
		{
			get
			{
				return this._Areas;
			}
		}

		// Token: 0x170006EE RID: 1774
		// (get) Token: 0x06001A51 RID: 6737 RVA: 0x0004B520 File Offset: 0x0004A520
		public _IArea[] _Areas
		{
			get
			{
				_IArea[] array = new _IArea[this.m_alAreas.Count];
				this.m_alAreas.CopyTo(array);
				return array;
			}
		}

		// Token: 0x06001A52 RID: 6738 RVA: 0x0004B54C File Offset: 0x0004A54C
		public IArea CreateArea(DataSegmentFlags dastFlags, KindOfArea kindof, int nMinimalSize, int nStartAddress, int nAllocationPlusinPercent)
		{
			AreaFlags areaflags = AreaFlags.DynamicSize;
			if (kindof == KindOfArea.Fixed)
			{
				areaflags = AreaFlags.Fixed;
			}
			return new Area(dastFlags, areaflags)
			{
				MinimalAreaSize = nMinimalSize,
				AllocationPlusInPercent = nAllocationPlusinPercent,
				StartAddress = nStartAddress
			};
		}

		// Token: 0x06001A53 RID: 6739 RVA: 0x0004B580 File Offset: 0x0004A580
		public uint CalculateChecksum()
		{
			ChecksumStream checksumStream = CompilerProxy.CreateChecksumStream(false);
			BinaryWriter binaryWriter = new BinaryWriter(checksumStream);
			binaryWriter.Write(this.m_bRetainInOwn);
			binaryWriter.Write(this.m_iDataSegmentSize);
			binaryWriter.Write(this.m_iCodeSegmentSize);
			binaryWriter.Write(this.m_iCodeSegmentPrologSize);
			binaryWriter.Write(this.m_iCodeSegmentHeaderSize);
			binaryWriter.Write(this.m_GlobalDataSize);
			binaryWriter.Write(this.m_CodeSize);
			binaryWriter.Write(this.m_MemoryDataSize);
			binaryWriter.Write(this.m_InputDataSize);
			binaryWriter.Write(this.m_OutputDataSize);
			binaryWriter.Write(this.m_RetainDataSize);
			binaryWriter.Write(this.m_iMaxDataSize);
			binaryWriter.Write(this.m_iMaxCodeSize);
			binaryWriter.Write(this.m_iPackMode);
			binaryWriter.Write(this.m_iMinSize);
			binaryWriter.Write(this.m_iStackAlignment);
			binaryWriter.Write(this.m_iMaxSizeForOnlineChange);
			binaryWriter.Write(this.m_bByteAddressing);
			binaryWriter.Write(this.m_bBitByteAddressing);
			binaryWriter.Write(this.m_bBitWordAddressing);
			binaryWriter.Write(this.m_bOCInOwnSegment);
			binaryWriter.Write(this.m_bRetainDynamic);
			binaryWriter.Write(this.m_bPersistentDynamic);
			binaryWriter.Write(this.m_iMinGranularity);
			foreach (_IArea iarea in this.m_alAreas)
			{
				binaryWriter.Write(iarea.AllocationPlusInPercent);
				binaryWriter.Write((uint)iarea.AreaFlags);
				binaryWriter.Write(iarea.Automatic);
				binaryWriter.Write((uint)iarea.DataSegmentFlags);
				binaryWriter.Write(iarea.Dynamic);
			}
			binaryWriter.Write(this.m_bAdditionalAreas);
			binaryWriter.Flush();
			checksumStream.Close();
			return checksumStream.Checksum;
		}

		// Token: 0x170006EF RID: 1775
		// (get) Token: 0x06001A54 RID: 6740 RVA: 0x0004B750 File Offset: 0x0004A750
		// (set) Token: 0x06001A55 RID: 6741 RVA: 0x0004B758 File Offset: 0x0004A758
		public int StaticAreaSize
		{
			get
			{
				return this.m_iStaticAreaSize;
			}
			set
			{
				this.m_iStaticAreaSize = value;
			}
		}

		// Token: 0x170006F0 RID: 1776
		// (get) Token: 0x06001A56 RID: 6742 RVA: 0x0004B761 File Offset: 0x0004A761
		// (set) Token: 0x06001A57 RID: 6743 RVA: 0x0004B769 File Offset: 0x0004A769
		public int AreaAlignment
		{
			get
			{
				return this.m_iAreaAlignment;
			}
			set
			{
				this.m_iAreaAlignment = value;
			}
		}

		// Token: 0x170006F1 RID: 1777
		// (get) Token: 0x06001A58 RID: 6744 RVA: 0x0004B772 File Offset: 0x0004A772
		[Obsolete("No longer used since Device Application is deprecated.")]
		public IDictionary<Guid, IList<_IDataSegment>> MappedDataSegmentsPerApp { get; }

		// Token: 0x170006F2 RID: 1778
		// (get) Token: 0x06001A59 RID: 6745 RVA: 0x0004B77A File Offset: 0x0004A77A
		[Obsolete("No longer used since Device Application is deprecated.")]
		public IList<_IDataSegment> MappedDataSegments { get; }

		// Token: 0x170006F3 RID: 1779
		// (get) Token: 0x06001A5A RID: 6746 RVA: 0x0004B782 File Offset: 0x0004A782
		// (set) Token: 0x06001A5B RID: 6747 RVA: 0x0004B78A File Offset: 0x0004A78A
		public int GlobalDataSize
		{
			get
			{
				return this.m_GlobalDataSize;
			}
			set
			{
				this.m_GlobalDataSize = value;
			}
		}

		// Token: 0x170006F4 RID: 1780
		// (get) Token: 0x06001A5C RID: 6748 RVA: 0x0004B793 File Offset: 0x0004A793
		// (set) Token: 0x06001A5D RID: 6749 RVA: 0x0004B79B File Offset: 0x0004A79B
		public int CodeSize
		{
			get
			{
				return this.m_CodeSize;
			}
			set
			{
				this.m_CodeSize = value;
			}
		}

		// Token: 0x170006F5 RID: 1781
		// (get) Token: 0x06001A5E RID: 6750 RVA: 0x0004B7A4 File Offset: 0x0004A7A4
		// (set) Token: 0x06001A5F RID: 6751 RVA: 0x0004B7AC File Offset: 0x0004A7AC
		public int MemoryDataSize
		{
			get
			{
				return this.m_MemoryDataSize;
			}
			set
			{
				this.m_MemoryDataSize = value;
			}
		}

		// Token: 0x170006F6 RID: 1782
		// (get) Token: 0x06001A60 RID: 6752 RVA: 0x0004B7B5 File Offset: 0x0004A7B5
		// (set) Token: 0x06001A61 RID: 6753 RVA: 0x0004B7BD File Offset: 0x0004A7BD
		public int InputDataSize
		{
			get
			{
				return this.m_InputDataSize;
			}
			set
			{
				this.m_InputDataSize = value;
			}
		}

		// Token: 0x170006F7 RID: 1783
		// (get) Token: 0x06001A62 RID: 6754 RVA: 0x0004B7C6 File Offset: 0x0004A7C6
		// (set) Token: 0x06001A63 RID: 6755 RVA: 0x0004B7CE File Offset: 0x0004A7CE
		public int OutputDataSize
		{
			get
			{
				return this.m_OutputDataSize;
			}
			set
			{
				this.m_OutputDataSize = value;
			}
		}

		// Token: 0x170006F8 RID: 1784
		// (get) Token: 0x06001A64 RID: 6756 RVA: 0x0004B7D7 File Offset: 0x0004A7D7
		// (set) Token: 0x06001A65 RID: 6757 RVA: 0x0004B7DF File Offset: 0x0004A7DF
		public int RetainDataSize
		{
			get
			{
				return this.m_RetainDataSize;
			}
			set
			{
				this.m_RetainDataSize = value;
			}
		}

		// Token: 0x06001A66 RID: 6758 RVA: 0x0004B7E8 File Offset: 0x0004A7E8
		public IArea CreateArea(DataSegmentFlags dastFlags, KindOfArea kindof, int nMinimalSize, int nStartAddress, int nAllocationPlusinPercent, int nMaximalAreaSize)
		{
			AreaFlags areaflags = AreaFlags.DynamicSize;
			if (kindof == KindOfArea.Fixed)
			{
				areaflags = AreaFlags.Fixed;
			}
			return new Area(dastFlags, areaflags)
			{
				MinimalAreaSize = nMinimalSize,
				AllocationPlusInPercent = nAllocationPlusinPercent,
				StartAddress = nStartAddress,
				Size = nMaximalAreaSize
			};
		}

		// Token: 0x0400054B RID: 1355
		private bool m_bRetainInOwn;

		// Token: 0x0400054C RID: 1356
		private int m_iDataSegmentSize;

		// Token: 0x0400054D RID: 1357
		private int m_iCodeSegmentSize;

		// Token: 0x0400054E RID: 1358
		private int m_iCodeSegmentPrologSize;

		// Token: 0x0400054F RID: 1359
		private int m_iCodeSegmentHeaderSize;

		// Token: 0x04000550 RID: 1360
		private int m_GlobalDataSize;

		// Token: 0x04000551 RID: 1361
		private int m_CodeSize;

		// Token: 0x04000552 RID: 1362
		private int m_MemoryDataSize;

		// Token: 0x04000553 RID: 1363
		private int m_InputDataSize;

		// Token: 0x04000554 RID: 1364
		private int m_OutputDataSize;

		// Token: 0x04000555 RID: 1365
		private int m_RetainDataSize;

		// Token: 0x04000556 RID: 1366
		private int m_iMaxDataSize;

		// Token: 0x04000557 RID: 1367
		private int m_iMaxCodeSize;

		// Token: 0x04000558 RID: 1368
		private int m_iPackMode;

		// Token: 0x04000559 RID: 1369
		private int m_iMinSize;

		// Token: 0x0400055A RID: 1370
		private int m_iStackAlignment;

		// Token: 0x0400055B RID: 1371
		private int m_iMaxSizeForOnlineChange;

		// Token: 0x0400055C RID: 1372
		private bool m_bByteAddressing;

		// Token: 0x0400055D RID: 1373
		private bool m_bBitByteAddressing;

		// Token: 0x0400055E RID: 1374
		private bool m_bBitWordAddressing;

		// Token: 0x0400055F RID: 1375
		private bool m_bOCInOwnSegment;

		// Token: 0x04000560 RID: 1376
		private bool m_bRetainDynamic;

		// Token: 0x04000561 RID: 1377
		private bool m_bPersistentDynamic;

		// Token: 0x04000562 RID: 1378
		private int m_iMinGranularity;

		// Token: 0x04000563 RID: 1379
		private readonly LList<_IArea> m_alAreas;

		// Token: 0x04000564 RID: 1380
		private int m_iStaticAreaSize;

		// Token: 0x04000565 RID: 1381
		private int m_iAreaAlignment;

		// Token: 0x04000566 RID: 1382
		private bool m_bAdditionalAreas;

		// Token: 0x04000567 RID: 1383
		private bool _bOneSRAMArea;
	}
}
