using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C5 RID: 453
	[TypeGuid("{40C3211D-4A89-419c-88A4-B26D33C2707E}")]
	[StorageVersion("3.3.0.0")]
	public class RelocationList : GenericObject2, _IRelocationList, IRelocationList2, IRelocationList
	{
		// Token: 0x06001FFE RID: 8190 RVA: 0x00058C19 File Offset: 0x00057C19
		public RelocationList.RelocationArea Get(int iIndex)
		{
			return this.m_alRelocationAreaList[iIndex] as RelocationList.RelocationArea;
		}

		// Token: 0x17000858 RID: 2136
		public RelocationList.RelocationArea this[int iArea]
		{
			get
			{
				for (int i = 0; i < this.m_alRelocationAreaList.Count; i++)
				{
					RelocationList.RelocationArea relocationArea = this.m_alRelocationAreaList[i] as RelocationList.RelocationArea;
					if (relocationArea.Area == iArea)
					{
						return relocationArea;
					}
				}
				return null;
			}
		}

		// Token: 0x17000859 RID: 2137
		// (get) Token: 0x06002000 RID: 8192 RVA: 0x00058C6D File Offset: 0x00057C6D
		public int Count
		{
			get
			{
				return this.m_alRelocationAreaList.Count;
			}
		}

		// Token: 0x06002001 RID: 8193 RVA: 0x00058C7C File Offset: 0x00057C7C
		public void AddRelocation(int iArea, int nOffset)
		{
			RelocationList.RelocationArea relocationArea = this[iArea];
			if (relocationArea == null)
			{
				relocationArea = new RelocationList.RelocationArea(iArea);
				this.m_alRelocationAreaList.Add(relocationArea);
			}
			relocationArea.AppendRelocation(nOffset);
		}

		// Token: 0x06002002 RID: 8194 RVA: 0x00058CAE File Offset: 0x00057CAE
		public void AddArea(RelocationList.RelocationArea ra)
		{
			this.m_alRelocationAreaList.Add(ra);
		}

		// Token: 0x06002003 RID: 8195 RVA: 0x00058CBC File Offset: 0x00057CBC
		public IRelocationList Duplicate()
		{
			RelocationList relocationList = new RelocationList();
			for (int i = 0; i < this.m_alRelocationAreaList.Count; i++)
			{
				RelocationList.RelocationArea relocationArea = this.m_alRelocationAreaList[i] as RelocationList.RelocationArea;
				relocationList.AddArea(relocationArea.Duplicate());
			}
			return relocationList;
		}

		// Token: 0x1700085A RID: 2138
		// (get) Token: 0x06002004 RID: 8196 RVA: 0x0003F515 File Offset: 0x0003E515
		public IRelocationList Empty
		{
			get
			{
				return new RelocationList();
			}
		}

		// Token: 0x06002005 RID: 8197 RVA: 0x00058D04 File Offset: 0x00057D04
		public void Merge(IRelocationList rl)
		{
			RelocationList relocationList = rl as RelocationList;
			for (int i = 0; i < relocationList.Count; i++)
			{
				RelocationList.RelocationArea relocationArea = relocationList.Get(i);
				RelocationList.RelocationArea relocationArea2 = this[relocationArea.Area];
				if (relocationArea2 != null)
				{
					relocationArea2.AppendRelocations(relocationArea);
				}
				else
				{
					this.m_alRelocationAreaList.Add(relocationArea);
				}
			}
		}

		// Token: 0x06002006 RID: 8198 RVA: 0x00058D58 File Offset: 0x00057D58
		public void AddOffset(int nOffset)
		{
			for (int i = 0; i < this.m_alRelocationAreaList.Count; i++)
			{
				(this.m_alRelocationAreaList[i] as RelocationList.RelocationArea).AddOffset(nOffset);
			}
		}

		// Token: 0x1700085B RID: 2139
		// (get) Token: 0x06002007 RID: 8199 RVA: 0x00058D94 File Offset: 0x00057D94
		public IRelocationAreaList[] RelocationAreaLists
		{
			get
			{
				RelocationList.RelocationArea[] array = new RelocationList.RelocationArea[this.m_alRelocationAreaList.Count];
				LList<IRelocationAreaList2> alRelocationAreaList = this.m_alRelocationAreaList;
				IRelocationAreaList2[] array2 = array;
				alRelocationAreaList.CopyTo(array2);
				return array;
			}
		}

		// Token: 0x1700085C RID: 2140
		// (get) Token: 0x06002008 RID: 8200 RVA: 0x00058DC3 File Offset: 0x00057DC3
		public IList<IRelocationAreaList2> RelocationAreaListsEx
		{
			get
			{
				return this.m_alRelocationAreaList;
			}
		}

		// Token: 0x06002009 RID: 8201 RVA: 0x00058DCC File Offset: 0x00057DCC
		public void Dump(BinaryWriter bw)
		{
			bw.Write(this.m_alRelocationAreaList.Count);
			for (int i = 0; i < this.m_alRelocationAreaList.Count; i++)
			{
				(this.m_alRelocationAreaList[i] as RelocationList.RelocationArea).Dump(bw);
			}
		}

		// Token: 0x04000650 RID: 1616
		[DefaultSerialization("arealist")]
		[StorageVersion("3.3.0.0")]
		private LList<IRelocationAreaList2> m_alRelocationAreaList = new LList<IRelocationAreaList2>();

		// Token: 0x020002CA RID: 714
		[TypeGuid("{51BC5DC9-3BC5-4cf9-80A8-0B044CC305B3}")]
		[StorageVersion("3.3.0.0")]
		public class Relocation : GenericObject2, IRelocation
		{
			// Token: 0x06002C39 RID: 11321 RVA: 0x0007472E File Offset: 0x0007372E
			public Relocation()
			{
			}

			// Token: 0x06002C3A RID: 11322 RVA: 0x0007473D File Offset: 0x0007373D
			public Relocation(int nOffset)
			{
				this.m_nOffset = nOffset;
			}

			// Token: 0x17000C27 RID: 3111
			// (get) Token: 0x06002C3B RID: 11323 RVA: 0x00074753 File Offset: 0x00073753
			// (set) Token: 0x06002C3C RID: 11324 RVA: 0x0007475B File Offset: 0x0007375B
			public int Offset
			{
				get
				{
					return this.m_nOffset;
				}
				set
				{
					this.m_nOffset = value;
				}
			}

			// Token: 0x06002C3D RID: 11325 RVA: 0x00074764 File Offset: 0x00073764
			public RelocationList.Relocation Duplicate()
			{
				return new RelocationList.Relocation(this.m_nOffset);
			}

			// Token: 0x040008EB RID: 2283
			[DefaultSerialization("offset")]
			[StorageVersion("3.3.0.0")]
			private int m_nOffset = -1;
		}

		// Token: 0x020002CB RID: 715
		[TypeGuid("{3E13FF7D-638B-4aed-B604-A84F530B3ED7}")]
		[StorageVersion("3.3.0.0")]
		public class RelocationArea : GenericObject2, IRelocationAreaList2, IRelocationAreaList
		{
			// Token: 0x06002C3E RID: 11326 RVA: 0x00074771 File Offset: 0x00073771
			public RelocationArea()
			{
			}

			// Token: 0x06002C3F RID: 11327 RVA: 0x0007478B File Offset: 0x0007378B
			public RelocationArea(int iArea)
			{
				this.m_iArea = iArea;
			}

			// Token: 0x17000C28 RID: 3112
			// (get) Token: 0x06002C40 RID: 11328 RVA: 0x000747AC File Offset: 0x000737AC
			public int Area
			{
				get
				{
					return this.m_iArea;
				}
			}

			// Token: 0x17000C29 RID: 3113
			// (get) Token: 0x06002C41 RID: 11329 RVA: 0x000747B4 File Offset: 0x000737B4
			public int Count
			{
				get
				{
					return this.m_alRelocations.Count;
				}
			}

			// Token: 0x06002C42 RID: 11330 RVA: 0x000747C1 File Offset: 0x000737C1
			public RelocationList.Relocation Get(int iIndex)
			{
				return this.m_alRelocations[iIndex] as RelocationList.Relocation;
			}

			// Token: 0x06002C43 RID: 11331 RVA: 0x000747D4 File Offset: 0x000737D4
			public RelocationList.RelocationArea Duplicate()
			{
				RelocationList.RelocationArea relocationArea = new RelocationList.RelocationArea(this.m_iArea);
				for (int i = 0; i < this.m_alRelocations.Count; i++)
				{
					RelocationList.Relocation relocation = this.m_alRelocations[i] as RelocationList.Relocation;
					relocationArea.AppendRelocation(relocation.Duplicate());
				}
				return relocationArea;
			}

			// Token: 0x06002C44 RID: 11332 RVA: 0x00074824 File Offset: 0x00073824
			public void AppendRelocation(int nOffset)
			{
				RelocationList.Relocation relocation = new RelocationList.Relocation(nOffset);
				this.m_alRelocations.Add(relocation);
			}

			// Token: 0x06002C45 RID: 11333 RVA: 0x00074844 File Offset: 0x00073844
			public void AppendRelocation(RelocationList.Relocation r)
			{
				this.m_alRelocations.Add(r);
			}

			// Token: 0x06002C46 RID: 11334 RVA: 0x00074854 File Offset: 0x00073854
			public void AppendRelocations(RelocationList.RelocationArea ra)
			{
				for (int i = 0; i < ra.Count; i++)
				{
					this.AppendRelocation(ra.Get(i));
				}
			}

			// Token: 0x06002C47 RID: 11335 RVA: 0x00074880 File Offset: 0x00073880
			public void AddOffset(int nOffset)
			{
				for (int i = 0; i < this.m_alRelocations.Count; i++)
				{
					(this.m_alRelocations[i] as RelocationList.Relocation).Offset += nOffset;
				}
			}

			// Token: 0x06002C48 RID: 11336 RVA: 0x000748C4 File Offset: 0x000738C4
			public void Dump(BinaryWriter bw)
			{
				bw.Write(this.m_alRelocations.Count);
				for (int i = 0; i < this.m_alRelocations.Count; i++)
				{
					RelocationList.Relocation relocation = this.m_alRelocations[i] as RelocationList.Relocation;
					bw.Write(relocation.Offset);
				}
			}

			// Token: 0x17000C2A RID: 3114
			// (get) Token: 0x06002C49 RID: 11337 RVA: 0x00074918 File Offset: 0x00073918
			public IRelocation[] Relocations
			{
				get
				{
					RelocationList.Relocation[] array = new RelocationList.Relocation[this.m_alRelocations.Count];
					LList<IRelocation> alRelocations = this.m_alRelocations;
					IRelocation[] array2 = array;
					alRelocations.CopyTo(array2);
					return array;
				}
			}

			// Token: 0x17000C2B RID: 3115
			// (get) Token: 0x06002C4A RID: 11338 RVA: 0x00074947 File Offset: 0x00073947
			public IList<IRelocation> RelocationsEx
			{
				get
				{
					return this.m_alRelocations;
				}
			}

			// Token: 0x040008EC RID: 2284
			[DefaultSerialization("area")]
			[StorageVersion("3.3.0.0")]
			private int m_iArea = -1;

			// Token: 0x040008ED RID: 2285
			[DefaultSerialization("relocs")]
			[StorageVersion("3.3.0.0")]
			private LList<IRelocation> m_alRelocations = new LList<IRelocation>();
		}
	}
}
