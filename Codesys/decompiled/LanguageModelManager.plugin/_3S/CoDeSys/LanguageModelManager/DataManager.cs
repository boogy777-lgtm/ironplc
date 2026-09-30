using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200012F RID: 303
	[TypeGuid("{a20044ae-acaa-44c9-83f9-750bbfb34c6a}")]
	[StorageVersion("3.3.0.0")]
	public class DataManager : GenericObject2, _IDataManager, IDataManager4, IDataManager3, IDataManager2, IDataManager, IDataManagerSerializable
	{
		// Token: 0x170006F9 RID: 1785
		// (get) Token: 0x06001A68 RID: 6760 RVA: 0x0004B85A File Offset: 0x0004A85A
		public ushort AreaCount
		{
			get
			{
				return (ushort)this._Areas.Count;
			}
		}

		// Token: 0x06001A69 RID: 6761 RVA: 0x0004B868 File Offset: 0x0004A868
		public _IArea GetArea(int i)
		{
			return (_IArea)this._Areas[i];
		}

		// Token: 0x06001A6A RID: 6762 RVA: 0x0004B87C File Offset: 0x0004A87C
		public override void AfterDeserialize()
		{
			this._AreaSegments.Clear();
			foreach (_IDataSegment idataSegment in this._DataSegments)
			{
				if (idataSegment.GetFlag(DataSegmentFlags.Area))
				{
					this._AreaSegments.Add(idataSegment);
				}
			}
		}

		// Token: 0x06001A6B RID: 6763 RVA: 0x0004B8E8 File Offset: 0x0004A8E8
		public _IArea GetAreaByIndex(int nAreaIndex)
		{
			foreach (IArea area in this._Areas)
			{
				if (area.Index == nAreaIndex)
				{
					return area as _IArea;
				}
			}
			return null;
		}

		// Token: 0x06001A6C RID: 6764 RVA: 0x0004B944 File Offset: 0x0004A944
		public void RemoveArea(_IArea area)
		{
			this._Areas.Remove(area);
		}

		// Token: 0x06001A6D RID: 6765 RVA: 0x0004B953 File Offset: 0x0004A953
		public void AddArea(_IArea area)
		{
			area.Index = (int)this.AreaCount + this.FirstArea;
			this._Areas.Add(area);
		}

		// Token: 0x170006FA RID: 1786
		// (get) Token: 0x06001A6E RID: 6766 RVA: 0x0004B974 File Offset: 0x0004A974
		public IArea[] Areas
		{
			get
			{
				IArea[] array = new IArea[this._Areas.Count];
				(this._Areas as LList<IArea>).CopyTo(array);
				return array;
			}
		}

		// Token: 0x170006FB RID: 1787
		// (get) Token: 0x06001A6F RID: 6767 RVA: 0x0004B9A4 File Offset: 0x0004A9A4
		// (set) Token: 0x06001A70 RID: 6768 RVA: 0x0004B9AC File Offset: 0x0004A9AC
		public int FirstArea
		{
			get
			{
				return this.m_iFirstArea;
			}
			set
			{
				this.m_iFirstArea = value;
			}
		}

		// Token: 0x170006FC RID: 1788
		// (get) Token: 0x06001A71 RID: 6769 RVA: 0x0004B9B5 File Offset: 0x0004A9B5
		public int Count
		{
			get
			{
				return this._DataSegments.Count;
			}
		}

		// Token: 0x170006FD RID: 1789
		// (get) Token: 0x06001A72 RID: 6770 RVA: 0x0004B9C2 File Offset: 0x0004A9C2
		public int DataSegmentSize
		{
			get
			{
				return this._MemorySettings.DataSegmentSize;
			}
		}

		// Token: 0x170006FE RID: 1790
		// (get) Token: 0x06001A73 RID: 6771 RVA: 0x0004B9CF File Offset: 0x0004A9CF
		public int PackMode
		{
			get
			{
				return this._MemorySettings.PackMode;
			}
		}

		// Token: 0x170006FF RID: 1791
		// (get) Token: 0x06001A74 RID: 6772 RVA: 0x0004B9DC File Offset: 0x0004A9DC
		public int MinSize
		{
			get
			{
				return this._MemorySettings.MinSize;
			}
		}

		// Token: 0x17000700 RID: 1792
		// (get) Token: 0x06001A75 RID: 6773 RVA: 0x0004B9E9 File Offset: 0x0004A9E9
		public int StackAlignment
		{
			get
			{
				return this._MemorySettings.StackAlignment;
			}
		}

		// Token: 0x17000701 RID: 1793
		// (get) Token: 0x06001A76 RID: 6774 RVA: 0x0004B9F6 File Offset: 0x0004A9F6
		public int CodeSegmentSize
		{
			get
			{
				return this._MemorySettings.CodeSegmentSize;
			}
		}

		// Token: 0x17000702 RID: 1794
		// (get) Token: 0x06001A77 RID: 6775 RVA: 0x0004BA03 File Offset: 0x0004AA03
		public bool RetainInOwnSegment
		{
			get
			{
				return this._MemorySettings.RetainInOwnSegment;
			}
		}

		// Token: 0x17000703 RID: 1795
		// (get) Token: 0x06001A78 RID: 6776 RVA: 0x0004BA10 File Offset: 0x0004AA10
		public bool ByteAddressing
		{
			get
			{
				return this._MemorySettings.ByteAddressing;
			}
		}

		// Token: 0x17000704 RID: 1796
		// (get) Token: 0x06001A79 RID: 6777 RVA: 0x0004BA1D File Offset: 0x0004AA1D
		public bool BitByteAddressing
		{
			get
			{
				return this._MemorySettings.BitByteAddressing;
			}
		}

		// Token: 0x17000705 RID: 1797
		// (get) Token: 0x06001A7A RID: 6778 RVA: 0x0004BA2A File Offset: 0x0004AA2A
		public bool BitWordAddressing
		{
			get
			{
				return this._MemorySettings.BitWordAddressing;
			}
		}

		// Token: 0x06001A7B RID: 6779 RVA: 0x0004BA38 File Offset: 0x0004AA38
		public bool CheckConsistency()
		{
			using (IEnumerator<_IDataSegment> enumerator = this._DataSegments.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.CheckConsistency())
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x17000706 RID: 1798
		public _IDataSegment this[DataSegmentFlags flags]
		{
			get
			{
				foreach (_IDataSegment idataSegment in this._DataSegments)
				{
					if (idataSegment.GetFlag(flags))
					{
						return idataSegment;
					}
				}
				return null;
			}
		}

		// Token: 0x06001A7D RID: 6781 RVA: 0x0004BAE4 File Offset: 0x0004AAE4
		public bool IsPersistentSupported()
		{
			foreach (_IDataSegment idataSegment in this._DataSegments)
			{
				if (idataSegment.GetFlag(DataSegmentFlags.Persistent) && idataSegment.Size > 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001A7E RID: 6782 RVA: 0x0004BB48 File Offset: 0x0004AB48
		public bool IsRetainSupported()
		{
			foreach (_IDataSegment idataSegment in this._DataSegments)
			{
				if (idataSegment.GetFlag(DataSegmentFlags.Retain) && idataSegment.Size > 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001A7F RID: 6783 RVA: 0x0004BBA8 File Offset: 0x0004ABA8
		[Obsolete("contains an error: for compiler version > 3.5.20.0 use MemoryCompiler.GetPreferredDataSegment instead")]
		public _IDataSegment GetPreferredDataSegment(DataSegmentFlags flags)
		{
			bool flag = (flags & DataSegmentFlags.Persistent) == DataSegmentFlags.Persistent;
			if ((flags & DataSegmentFlags.Retain) != DataSegmentFlags.None && !this.RetainInOwnSegment)
			{
				flags &= ~DataSegmentFlags.Retain;
			}
			try
			{
				for (ushort num = 0; num < (ushort)this._DataSegments.Count; num += 1)
				{
					_IDataSegment idataSegment = this._DataSegments[(int)num];
					if (((flags & DataSegmentFlags.Retain) == DataSegmentFlags.None || flag || !this.RetainInOwnSegment || this._MemorySettings.RetainDynamic || idataSegment.Flags == DataSegmentFlags.Retain) && idataSegment.GetFlag(flags))
					{
						return idataSegment;
					}
				}
			}
			catch
			{
				return null;
			}
			return null;
		}

		// Token: 0x06001A80 RID: 6784 RVA: 0x0004BC50 File Offset: 0x0004AC50
		public _IDataSegment[] AllDataSegmentsByFlag(DataSegmentFlags flags)
		{
			ArrayList arrayList = new ArrayList();
			foreach (_IDataSegment idataSegment in this._DataSegments)
			{
				if (idataSegment.GetFlag(flags))
				{
					arrayList.Add(idataSegment);
				}
			}
			DataSegment[] array = new DataSegment[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}

		// Token: 0x17000707 RID: 1799
		public _IDataSegment this[ushort usRefId]
		{
			get
			{
				return this._DataSegments[(int)usRefId] as DataSegment;
			}
		}

		// Token: 0x17000708 RID: 1800
		// (get) Token: 0x06001A82 RID: 6786 RVA: 0x0004BCDB File Offset: 0x0004ACDB
		// (set) Token: 0x06001A83 RID: 6787 RVA: 0x0004BCE3 File Offset: 0x0004ACE3
		public _IMemorySettings _MemorySettings
		{
			get
			{
				return this.m_memset;
			}
			set
			{
				this.m_memset = (value as MemorySettings);
			}
		}

		// Token: 0x17000709 RID: 1801
		// (get) Token: 0x06001A84 RID: 6788 RVA: 0x0004BCF1 File Offset: 0x0004ACF1
		// (set) Token: 0x06001A85 RID: 6789 RVA: 0x0004BCF9 File Offset: 0x0004ACF9
		public _IDataManager Reference
		{
			get
			{
				return this.m_dmReference;
			}
			set
			{
				this.m_dmReference = (value as DataManager);
			}
		}

		// Token: 0x1700070A RID: 1802
		public _IDataSegment this[int i]
		{
			get
			{
				return this._DataSegments[i];
			}
		}

		// Token: 0x06001A87 RID: 6791 RVA: 0x0004BD15 File Offset: 0x0004AD15
		public _IDataSegment GetAreaSegment(int i)
		{
			return this._AreaSegments[i];
		}

		// Token: 0x1700070B RID: 1803
		// (get) Token: 0x06001A88 RID: 6792 RVA: 0x0004BD23 File Offset: 0x0004AD23
		public _IDataSegment[] AreaSegments
		{
			get
			{
				return (this._AreaSegments as LList<_IDataSegment>).ToArray();
			}
		}

		// Token: 0x06001A89 RID: 6793 RVA: 0x0004BD35 File Offset: 0x0004AD35
		public bool IsEmptyArea(int iAreaIndex)
		{
			return CompilerProxy.IsEmptyArea(this, iAreaIndex);
		}

		// Token: 0x1700070C RID: 1804
		// (get) Token: 0x06001A8A RID: 6794 RVA: 0x0004BD40 File Offset: 0x0004AD40
		public bool HasConstantSegment
		{
			get
			{
				try
				{
					for (ushort num = 0; num < (ushort)this._DataSegments.Count; num += 1)
					{
						if (this._DataSegments[(int)num].GetFlag(DataSegmentFlags.Constant))
						{
							return true;
						}
					}
				}
				catch
				{
				}
				return false;
			}
		}

		// Token: 0x1700070D RID: 1805
		// (get) Token: 0x06001A8B RID: 6795 RVA: 0x0004BD98 File Offset: 0x0004AD98
		// (set) Token: 0x06001A8C RID: 6796 RVA: 0x0004BDA0 File Offset: 0x0004ADA0
		public IList<_IDataSegment> _DataSegments
		{
			get
			{
				return this.m_alDataSegments;
			}
			set
			{
				this.m_alDataSegments = new LList<_IDataSegment>(value);
			}
		}

		// Token: 0x1700070E RID: 1806
		// (get) Token: 0x06001A8D RID: 6797 RVA: 0x0004BDAE File Offset: 0x0004ADAE
		public IEnumerable<IDataSegment> DataSegments
		{
			get
			{
				foreach (IDataSegment dataSegment in this.m_alDataSegments)
				{
					yield return dataSegment;
				}
				IEnumerator<_IDataSegment> enumerator = null;
				yield break;
				yield break;
			}
		}

		// Token: 0x1700070F RID: 1807
		// (get) Token: 0x06001A8E RID: 6798 RVA: 0x0004BDBE File Offset: 0x0004ADBE
		public IList<_IDataSegment> _AreaSegments
		{
			get
			{
				return this.m_alAreaSegments;
			}
		}

		// Token: 0x17000710 RID: 1808
		// (get) Token: 0x06001A8F RID: 6799 RVA: 0x0004BDC6 File Offset: 0x0004ADC6
		// (set) Token: 0x06001A90 RID: 6800 RVA: 0x0004BDCE File Offset: 0x0004ADCE
		public IList<IArea> _Areas
		{
			get
			{
				return this.m_alAreas;
			}
			set
			{
				this.m_alAreas = new LList<IArea>(value);
			}
		}

		// Token: 0x06001A91 RID: 6801 RVA: 0x0004BDDC File Offset: 0x0004ADDC
		public void Dump(TextWriter textwriter)
		{
			textwriter.WriteLine("MemorySettings:");
			string value = string.Format("SegmentSize: {0}, RetainInOwn: {1}", this.DataSegmentSize, this.RetainInOwnSegment);
			textwriter.WriteLine(value);
			for (int i = 0; i < this._DataSegments.Count; i++)
			{
				(this._DataSegments[i] as DataSegment).Dump(textwriter, i);
			}
		}

		// Token: 0x0400056B RID: 1387
		[DefaultSerialization("DataSegments")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private LList<_IDataSegment> m_alDataSegments = new LList<_IDataSegment>();

		// Token: 0x0400056C RID: 1388
		[Obfuscation(Feature = "rename")]
		private readonly LList<_IDataSegment> m_alAreaSegments = new LList<_IDataSegment>();

		// Token: 0x0400056D RID: 1389
		[Obfuscation(Feature = "rename")]
		private MemorySettings m_memset = new MemorySettings(false);

		// Token: 0x0400056E RID: 1390
		[DefaultSerialization("Areas")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.0.255")]
		private LList<IArea> m_alAreas = new LList<IArea>(1);

		// Token: 0x0400056F RID: 1391
		[DefaultSerialization("FirstArea")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iFirstArea;

		// Token: 0x04000570 RID: 1392
		[Obfuscation(Feature = "rename")]
		private DataManager m_dmReference;
	}
}
