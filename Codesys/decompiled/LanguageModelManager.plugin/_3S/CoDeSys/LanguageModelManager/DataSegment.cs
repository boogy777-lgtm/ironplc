using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200012C RID: 300
	[TypeGuid("{e0d57300-b431-4917-bfe4-fc6030a266ee}")]
	[StorageVersion("3.3.0.0")]
	public class DataSegment : GenericObject2, _IDataSegment, IDataSegment
	{
		// Token: 0x060019EB RID: 6635 RVA: 0x0004A5BA File Offset: 0x000495BA
		public DataSegment()
		{
		}

		// Token: 0x060019EC RID: 6636 RVA: 0x0004A5CD File Offset: 0x000495CD
		public DataSegment(ushort usArea, int iAddress, int iSize, DataSegmentFlags flags)
		{
			this.Area = usArea;
			this.Address = iAddress;
			this.m_nSize = iSize;
			this.m_flags = flags;
			this.MemMan = new MemMan(iSize, iAddress);
		}

		// Token: 0x170006BE RID: 1726
		// (get) Token: 0x060019ED RID: 6637 RVA: 0x0004A60A File Offset: 0x0004960A
		public DataSegmentFlags Flags
		{
			get
			{
				return this.m_flags;
			}
		}

		// Token: 0x060019EE RID: 6638 RVA: 0x0004A612 File Offset: 0x00049612
		public bool CheckConsistency()
		{
			return this.MemMan.CheckConsistency();
		}

		// Token: 0x060019EF RID: 6639 RVA: 0x0004A61F File Offset: 0x0004961F
		public bool GetFlag(DataSegmentFlags dsFlag)
		{
			return (this.m_flags & dsFlag) == dsFlag;
		}

		// Token: 0x060019F0 RID: 6640 RVA: 0x0004A62C File Offset: 0x0004962C
		public void SetFlag(DataSegmentFlags dsFlag, bool bSetTrue)
		{
			if (bSetTrue)
			{
				this.m_flags |= dsFlag;
				return;
			}
			this.m_flags &= ~dsFlag;
		}

		// Token: 0x170006BF RID: 1727
		// (get) Token: 0x060019F1 RID: 6641 RVA: 0x0004A650 File Offset: 0x00049650
		// (set) Token: 0x060019F2 RID: 6642 RVA: 0x0004A658 File Offset: 0x00049658
		public bool LogByFlag
		{
			get
			{
				return this.m_bLogByFlag;
			}
			set
			{
				if (!this.m_bLogByFlag && value)
				{
					this.MemMansToFlags = new LDictionary<DataSegmentFlags, _IMemoryManager>();
				}
				this.m_bLogByFlag = value;
			}
		}

		// Token: 0x170006C0 RID: 1728
		// (get) Token: 0x060019F3 RID: 6643 RVA: 0x0004A679 File Offset: 0x00049679
		public int SizeAllocated
		{
			get
			{
				return this.MemMan.SizeAllocated;
			}
		}

		// Token: 0x170006C1 RID: 1729
		// (get) Token: 0x060019F4 RID: 6644 RVA: 0x0004A686 File Offset: 0x00049686
		// (set) Token: 0x060019F5 RID: 6645 RVA: 0x0004A68E File Offset: 0x0004968E
		public _IMemoryManager MemMan
		{
			get
			{
				return this.m_memman;
			}
			set
			{
				this.m_memman = (value as MemMan);
			}
		}

		// Token: 0x170006C2 RID: 1730
		// (get) Token: 0x060019F6 RID: 6646 RVA: 0x0004A69C File Offset: 0x0004969C
		public int MaxContiguosMemory
		{
			get
			{
				return this.MemMan.MaxContiguosMemory;
			}
		}

		// Token: 0x170006C3 RID: 1731
		// (get) Token: 0x060019F7 RID: 6647 RVA: 0x0004A6A9 File Offset: 0x000496A9
		public IEnumerable<IMemManGap> Gaps
		{
			get
			{
				int num;
				for (int i = 0; i < this.MemMan.Count; i = num + 1)
				{
					yield return this.MemMan[i];
					num = i;
				}
				yield break;
			}
		}

		// Token: 0x060019F8 RID: 6648 RVA: 0x0004A6BC File Offset: 0x000496BC
		public void Dump(TextWriter textwriter, int iIndex)
		{
			string value = string.Format("Segment {0}: Area {1}, Address {2}, Size {3}, ", new object[]
			{
				iIndex,
				this.Area,
				this.Address,
				this.m_nSize
			});
			textwriter.Write(value);
			if (this.IsData)
			{
				textwriter.WriteLine("Data");
			}
			else if (this.IsConstant)
			{
				textwriter.WriteLine("Constant");
			}
			else if (this.IsInput)
			{
				textwriter.WriteLine("Input");
			}
			else if (this.IsOutput)
			{
				textwriter.WriteLine("Output");
			}
			else if (this.IsMemory)
			{
				textwriter.WriteLine("Memory");
			}
			else if (this.IsRetain)
			{
				textwriter.WriteLine("Retain");
			}
			else if (this.IsCode)
			{
				textwriter.WriteLine("Code");
			}
			int num = this.Size;
			for (int i = 0; i < this.MemMan.Count; i++)
			{
				_IMemManGap imemManGap = this.MemMan[i];
				num -= imemManGap.Size;
			}
			textwriter.WriteLine("Allocated space: {0} Bytes", num);
			textwriter.WriteLine("Highest used address: {0} Bytes", this.MemMan.SizeAllocated);
			value = string.Format("Gaps: {0}\toffset\tsize{1}", this.MemMan.Count, Environment.NewLine);
			textwriter.Write(value);
			for (int j = 0; j < this.MemMan.Count; j++)
			{
				_IMemManGap imemManGap2 = this.MemMan[j];
				value = string.Format("\t{0}\t{1}{2}", imemManGap2.Offset, imemManGap2.Size, Environment.NewLine);
				textwriter.Write(value);
			}
		}

		// Token: 0x170006C4 RID: 1732
		// (get) Token: 0x060019F9 RID: 6649 RVA: 0x0004A883 File Offset: 0x00049883
		public bool IsData
		{
			get
			{
				return this.GetFlag(DataSegmentFlags.Data);
			}
		}

		// Token: 0x170006C5 RID: 1733
		// (get) Token: 0x060019FA RID: 6650 RVA: 0x0004A88C File Offset: 0x0004988C
		public bool IsConstant
		{
			get
			{
				return this.GetFlag(DataSegmentFlags.Constant);
			}
		}

		// Token: 0x170006C6 RID: 1734
		// (get) Token: 0x060019FB RID: 6651 RVA: 0x0004A895 File Offset: 0x00049895
		public bool IsInput
		{
			get
			{
				return this.GetFlag(DataSegmentFlags.Input);
			}
		}

		// Token: 0x170006C7 RID: 1735
		// (get) Token: 0x060019FC RID: 6652 RVA: 0x0004A89E File Offset: 0x0004989E
		public bool IsOutput
		{
			get
			{
				return this.GetFlag(DataSegmentFlags.Output);
			}
		}

		// Token: 0x170006C8 RID: 1736
		// (get) Token: 0x060019FD RID: 6653 RVA: 0x0004A8A7 File Offset: 0x000498A7
		public bool IsMemory
		{
			get
			{
				return this.GetFlag(DataSegmentFlags.Memory);
			}
		}

		// Token: 0x170006C9 RID: 1737
		// (get) Token: 0x060019FE RID: 6654 RVA: 0x0004A8B1 File Offset: 0x000498B1
		public bool IsRetain
		{
			get
			{
				return this.GetFlag(DataSegmentFlags.Retain);
			}
		}

		// Token: 0x170006CA RID: 1738
		// (get) Token: 0x060019FF RID: 6655 RVA: 0x0004A8BB File Offset: 0x000498BB
		public bool IsCode
		{
			get
			{
				return this.GetFlag(DataSegmentFlags.Code);
			}
		}

		// Token: 0x170006CB RID: 1739
		// (get) Token: 0x06001A00 RID: 6656 RVA: 0x0004A8C5 File Offset: 0x000498C5
		// (set) Token: 0x06001A01 RID: 6657 RVA: 0x0004A8CD File Offset: 0x000498CD
		public int Size
		{
			get
			{
				return this.m_nSize;
			}
			set
			{
				this.m_nSize = value;
			}
		}

		// Token: 0x170006CC RID: 1740
		// (get) Token: 0x06001A02 RID: 6658 RVA: 0x0004A8D6 File Offset: 0x000498D6
		// (set) Token: 0x06001A03 RID: 6659 RVA: 0x0004A8DE File Offset: 0x000498DE
		public int DPTableOffset
		{
			get
			{
				return this.m_nDPTableOffset;
			}
			set
			{
				this.m_nDPTableOffset = value;
			}
		}

		// Token: 0x170006CD RID: 1741
		// (get) Token: 0x06001A04 RID: 6660 RVA: 0x0004A8E7 File Offset: 0x000498E7
		// (set) Token: 0x06001A05 RID: 6661 RVA: 0x0004A8EF File Offset: 0x000498EF
		public ushort Area
		{
			get
			{
				return this.m_usArea;
			}
			set
			{
				this.m_usArea = value;
			}
		}

		// Token: 0x170006CE RID: 1742
		// (get) Token: 0x06001A06 RID: 6662 RVA: 0x0004A8F8 File Offset: 0x000498F8
		// (set) Token: 0x06001A07 RID: 6663 RVA: 0x0004A900 File Offset: 0x00049900
		public IDictionary<DataSegmentFlags, _IMemoryManager> MemMansToFlags
		{
			get
			{
				return this.m_htMemMansToFlags;
			}
			set
			{
				this.m_htMemMansToFlags = (value as LDictionary<DataSegmentFlags, _IMemoryManager>);
			}
		}

		// Token: 0x170006CF RID: 1743
		// (get) Token: 0x06001A08 RID: 6664 RVA: 0x0004A90E File Offset: 0x0004990E
		// (set) Token: 0x06001A09 RID: 6665 RVA: 0x0004A916 File Offset: 0x00049916
		public int Address
		{
			get
			{
				return this.m_nAddress;
			}
			set
			{
				this.m_nAddress = value;
			}
		}

		// Token: 0x04000541 RID: 1345
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Area")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ushort m_usArea;

		// Token: 0x04000542 RID: 1346
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Address")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nAddress;

		// Token: 0x04000543 RID: 1347
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("DPTableOffset")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nDPTableOffset;

		// Token: 0x04000544 RID: 1348
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Size")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nSize;

		// Token: 0x04000545 RID: 1349
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("Flags")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private DataSegmentFlags m_flags;

		// Token: 0x04000546 RID: 1350
		[DefaultDuplication(DuplicationMethod.Deep)]
		[DefaultSerialization("Memman")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private MemMan m_memman;

		// Token: 0x04000547 RID: 1351
		[DefaultDuplication(DuplicationMethod.Deep)]
		[DefaultSerialization("SegmentSize")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nSegmentSize = MemorySettings.NoSegmentation;

		// Token: 0x04000548 RID: 1352
		[DefaultDuplication(DuplicationMethod.Deep)]
		[DefaultSerialization("HTMemMansToFlags")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.9.255")]
		[Obfuscation(Feature = "rename")]
		private LDictionary<DataSegmentFlags, _IMemoryManager> m_htMemMansToFlags;

		// Token: 0x04000549 RID: 1353
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("LogByFlag")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bLogByFlag;
	}
}
