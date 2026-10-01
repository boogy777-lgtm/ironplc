using System;
using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000254 RID: 596
	internal class CompiledSignatureInformation
	{
		// Token: 0x17000B19 RID: 2841
		// (get) Token: 0x060027EC RID: 10220 RVA: 0x000642CC File Offset: 0x000632CC
		private FunctionBlockCompiledInformation FBInfoCreate
		{
			get
			{
				FunctionBlockCompiledInformation result;
				if ((result = this.m_fbcompinfo) == null)
				{
					result = (this.m_fbcompinfo = new FunctionBlockCompiledInformation());
				}
				return result;
			}
		}

		// Token: 0x060027ED RID: 10221 RVA: 0x000642F4 File Offset: 0x000632F4
		internal CompiledSignatureInformation Duplicate()
		{
			CompiledSignatureInformation compiledSignatureInformation = new CompiledSignatureInformation
			{
				m_iParentSignatureId = this.m_iParentSignatureId,
				m_nId = this.m_nId
			};
			if (this.m_htVariablesById != null)
			{
				compiledSignatureInformation.m_htVariablesById = new LDictionary<int, _IVariable>();
				foreach (int num in this.m_htVariablesById.Keys)
				{
					compiledSignatureInformation.m_htVariablesById.Add(num, this.m_htVariablesById[num]);
				}
			}
			compiledSignatureInformation.m_iDPOffset = this.m_iDPOffset;
			if (this.m_locationFP != null)
			{
				compiledSignatureInformation.m_locationFP = new DataLocation(this.m_locationFP.Area, this.m_locationFP.Offset);
			}
			if (this.m_alCallers != null)
			{
				compiledSignatureInformation.m_alCallers = new LList<int>(this.m_alCallers);
			}
			if (this.m_hashCallees != null)
			{
				compiledSignatureInformation.m_hashCallees = new HashSet<uint>(this.m_hashCallees);
			}
			if (this.m_byTaskIndexList != null)
			{
				compiledSignatureInformation.m_byTaskIndexList = (this.m_byTaskIndexList.Clone() as byte[]);
			}
			compiledSignatureInformation.m_nSize = this.m_nSize;
			compiledSignatureInformation.m_nCalleeSize = this.m_nCalleeSize;
			if (this.m_fbcompinfo != null)
			{
				compiledSignatureInformation.m_fbcompinfo = this.m_fbcompinfo.Duplicate();
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352000 && this.m_vftable != null && compiledSignatureInformation.m_vftable != null && this.m_vftable.DataLocation != null)
			{
				compiledSignatureInformation.m_vftable.DataLocation = new DataLocation(this.m_vftable.DataLocation.Area, this.m_vftable.DataLocation.Offset);
			}
			return compiledSignatureInformation;
		}

		// Token: 0x17000B1A RID: 2842
		// (get) Token: 0x060027EE RID: 10222 RVA: 0x000644A4 File Offset: 0x000634A4
		// (set) Token: 0x060027EF RID: 10223 RVA: 0x000644BB File Offset: 0x000634BB
		internal int[] m_aiInterfaceIds
		{
			get
			{
				if (this.m_fbcompinfo == null)
				{
					return null;
				}
				return this.m_fbcompinfo.m_aiInterfaceIds;
			}
			set
			{
				if (value != null)
				{
					this.FBInfoCreate.m_aiInterfaceIds = value;
				}
			}
		}

		// Token: 0x17000B1B RID: 2843
		// (get) Token: 0x060027F0 RID: 10224 RVA: 0x000644CC File Offset: 0x000634CC
		// (set) Token: 0x060027F1 RID: 10225 RVA: 0x000644E3 File Offset: 0x000634E3
		internal Hashtable m_htDeclarers
		{
			get
			{
				if (this.m_fbcompinfo == null)
				{
					return null;
				}
				return this.m_fbcompinfo.m_htDeclarers;
			}
			set
			{
				if (value != null)
				{
					this.FBInfoCreate.m_htDeclarers = value;
				}
			}
		}

		// Token: 0x17000B1C RID: 2844
		// (get) Token: 0x060027F2 RID: 10226 RVA: 0x000644F4 File Offset: 0x000634F4
		// (set) Token: 0x060027F3 RID: 10227 RVA: 0x0006450B File Offset: 0x0006350B
		internal LDictionary<int, int> m_htReferencer
		{
			get
			{
				if (this.m_fbcompinfo == null)
				{
					return null;
				}
				return this.m_fbcompinfo.m_htReferencer;
			}
			set
			{
				if (value != null)
				{
					this.FBInfoCreate.m_htReferencer = value;
				}
			}
		}

		// Token: 0x17000B1D RID: 2845
		// (get) Token: 0x060027F4 RID: 10228 RVA: 0x0006451C File Offset: 0x0006351C
		// (set) Token: 0x060027F5 RID: 10229 RVA: 0x00064533 File Offset: 0x00063533
		internal VFTable m_vftable
		{
			get
			{
				if (this.m_fbcompinfo == null)
				{
					return null;
				}
				return this.m_fbcompinfo.m_vftable;
			}
			set
			{
				if (value != null)
				{
					this.FBInfoCreate.m_vftable = value;
				}
			}
		}

		// Token: 0x17000B1E RID: 2846
		// (get) Token: 0x060027F6 RID: 10230 RVA: 0x00064544 File Offset: 0x00063544
		// (set) Token: 0x060027F7 RID: 10231 RVA: 0x0006455F File Offset: 0x0006355F
		internal int m_iBaseSignatureid
		{
			get
			{
				if (this.m_fbcompinfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_fbcompinfo.m_iBaseSignatureid;
			}
			set
			{
				if (value != Common.InvalidID)
				{
					this.FBInfoCreate.m_iBaseSignatureid = value;
				}
			}
		}

		// Token: 0x060027F8 RID: 10232 RVA: 0x00064575 File Offset: 0x00063575
		internal void ResetBaseSignatureId()
		{
			if (this.m_fbcompinfo != null)
			{
				this.m_fbcompinfo.m_iBaseSignatureid = -1;
			}
		}

		// Token: 0x17000B1F RID: 2847
		// (get) Token: 0x060027F9 RID: 10233 RVA: 0x0006458B File Offset: 0x0006358B
		// (set) Token: 0x060027FA RID: 10234 RVA: 0x000645B6 File Offset: 0x000635B6
		internal CaseInsensitiveHashtable m_htSignatures
		{
			get
			{
				if (this.m_fbcompinfo == null)
				{
					return null;
				}
				if (this.m_fbcompinfo._subSignatureTable == null)
				{
					return null;
				}
				return this.m_fbcompinfo._subSignatureTable.GetSubSignaturesTable();
			}
			set
			{
				if (value != null)
				{
					this.FBInfoCreate.SetSubSignatureTable(value);
				}
			}
		}

		// Token: 0x17000B20 RID: 2848
		// (get) Token: 0x060027FB RID: 10235 RVA: 0x000645C7 File Offset: 0x000635C7
		// (set) Token: 0x060027FC RID: 10236 RVA: 0x000645DE File Offset: 0x000635DE
		internal SubSignatureTable SubSignatureTable
		{
			get
			{
				if (this.m_fbcompinfo == null)
				{
					return null;
				}
				return this.m_fbcompinfo._subSignatureTable;
			}
			set
			{
				if (value != null)
				{
					this.FBInfoCreate._subSignatureTable = value;
				}
			}
		}

		// Token: 0x04000781 RID: 1921
		internal int m_iParentSignatureId = Common.InvalidID;

		// Token: 0x04000782 RID: 1922
		internal int m_nId = Common.InvalidID;

		// Token: 0x04000783 RID: 1923
		internal LDictionary<int, _IVariable> m_htVariablesById;

		// Token: 0x04000784 RID: 1924
		internal int m_iDPOffset = SignatureConstant.InvalidOffset;

		// Token: 0x04000785 RID: 1925
		internal IDataLocation m_locationFP;

		// Token: 0x04000786 RID: 1926
		internal LList<int> m_alCallers;

		// Token: 0x04000787 RID: 1927
		internal HashSet<uint> m_hashCallees;

		// Token: 0x04000788 RID: 1928
		internal byte[] m_byTaskIndexList;

		// Token: 0x04000789 RID: 1929
		internal int m_nSize;

		// Token: 0x0400078A RID: 1930
		internal int m_HighestUsedOffset;

		// Token: 0x0400078B RID: 1931
		internal int m_nCalleeSize;

		// Token: 0x0400078C RID: 1932
		private FunctionBlockCompiledInformation m_fbcompinfo;
	}
}
