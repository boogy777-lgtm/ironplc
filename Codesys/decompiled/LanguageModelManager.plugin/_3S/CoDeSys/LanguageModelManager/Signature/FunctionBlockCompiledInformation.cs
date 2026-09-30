using System;
using System.Collections;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000256 RID: 598
	internal class FunctionBlockCompiledInformation
	{
		// Token: 0x06002810 RID: 10256 RVA: 0x0006471D File Offset: 0x0006371D
		internal void SetSubSignatureTable(CaseInsensitiveHashtable htSignatures)
		{
			if (this._subSignatureTable == null)
			{
				this._subSignatureTable = new SubSignatureTable();
			}
			this._subSignatureTable.SetSubSignatures(htSignatures);
		}

		// Token: 0x06002811 RID: 10257 RVA: 0x00064740 File Offset: 0x00063740
		internal FunctionBlockCompiledInformation Duplicate()
		{
			FunctionBlockCompiledInformation functionBlockCompiledInformation = new FunctionBlockCompiledInformation();
			if (this.m_htDeclarers != null)
			{
				functionBlockCompiledInformation.m_htDeclarers = new Hashtable();
				foreach (object obj in this.m_htDeclarers.Keys)
				{
					int num = (int)obj;
					functionBlockCompiledInformation.m_htDeclarers.Add(num, this.m_htDeclarers[num]);
				}
			}
			if (this.m_htReferencer != null)
			{
				functionBlockCompiledInformation.m_htReferencer = new LDictionary<int, int>();
				foreach (int num2 in this.m_htReferencer.Keys)
				{
					functionBlockCompiledInformation.m_htReferencer[num2] = num2;
				}
			}
			functionBlockCompiledInformation.m_iBaseSignatureid = this.m_iBaseSignatureid;
			functionBlockCompiledInformation._subSignatureTable = this._subSignatureTable.Duplicate();
			if (this.m_aiInterfaceIds != null)
			{
				functionBlockCompiledInformation.m_aiInterfaceIds = (this.m_aiInterfaceIds.Clone() as int[]);
			}
			if (this.m_vftable != null)
			{
				functionBlockCompiledInformation.m_vftable = this.m_vftable.Clone();
			}
			return functionBlockCompiledInformation;
		}

		// Token: 0x0400078F RID: 1935
		internal SubSignatureTable _subSignatureTable;

		// Token: 0x04000790 RID: 1936
		internal Hashtable m_htDeclarers;

		// Token: 0x04000791 RID: 1937
		internal LDictionary<int, int> m_htReferencer;

		// Token: 0x04000792 RID: 1938
		internal VFTable m_vftable;

		// Token: 0x04000793 RID: 1939
		internal int m_iBaseSignatureid = Common.InvalidID;

		// Token: 0x04000794 RID: 1940
		internal int[] m_aiInterfaceIds;
	}
}
