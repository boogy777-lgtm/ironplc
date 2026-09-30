using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Variable
{
	// Token: 0x020001B1 RID: 433
	public class VariableCompiledInfo
	{
		// Token: 0x06001EEE RID: 7918 RVA: 0x00054E64 File Offset: 0x00053E64
		public void AddModifyingCrossReference(int nCodeId)
		{
			object modifyingAccessLock = this._modifyingAccessLock;
			lock (modifyingAccessLock)
			{
				this._modifyingAccesses.Add(nCodeId);
			}
		}

		// Token: 0x06001EEF RID: 7919 RVA: 0x00054EAC File Offset: 0x00053EAC
		public int[] GetModifyingCrossReferences()
		{
			object modifyingAccessLock = this._modifyingAccessLock;
			int[] result;
			lock (modifyingAccessLock)
			{
				result = this._modifyingAccesses.ToArray<int>();
			}
			return result;
		}

		// Token: 0x06001EF0 RID: 7920 RVA: 0x00054EF4 File Offset: 0x00053EF4
		internal VariableCompiledInfo Duplicate()
		{
			VariableCompiledInfo variableCompiledInfo = new VariableCompiledInfo();
			variableCompiledInfo.m_nId = this.m_nId;
			if (this.m_hsCrossRefs != null)
			{
				variableCompiledInfo.m_hsCrossRefs = new LHashSet<int>();
				foreach (int num in this.m_hsCrossRefs)
				{
					variableCompiledInfo.m_hsCrossRefs.Add(num);
				}
			}
			object modifyingAccessLock = this._modifyingAccessLock;
			lock (modifyingAccessLock)
			{
				foreach (int nCodeId in this._modifyingAccesses)
				{
					variableCompiledInfo.AddModifyingCrossReference(nCodeId);
				}
			}
			if (this.m_location != null)
			{
				if (this.m_location.IsBitLocation && !this.m_location.IsRelativ)
				{
					variableCompiledInfo.m_location = (LanguageModelBuilder.Singleton.CreateBitDataLocation(this.m_location.Area, this.m_location.Offset, this.m_location.BitNr) as _IDataLocation);
				}
				else if (!this.m_location.IsBitLocation && this.m_location.IsRelativ)
				{
					variableCompiledInfo.m_location = (LanguageModelBuilder.Singleton.CreateRelativeDataLocation(this.m_location.Offset, (this.m_location as _IRelativeDataLocation).Flags) as _IDataLocation);
				}
				else if (this.m_location.IsBitLocation && this.m_location.IsRelativ)
				{
					variableCompiledInfo.m_location = (LanguageModelBuilder.Singleton.CreateRelativeBitDataLocation(this.m_location.Offset, this.m_location.BitNr) as _IDataLocation);
				}
				else
				{
					variableCompiledInfo.m_location = (LanguageModelBuilder.Singleton.CreateDataLocation(this.m_location.Area, this.m_location.Offset) as _IDataLocation);
				}
			}
			return variableCompiledInfo;
		}

		// Token: 0x0400060B RID: 1547
		internal int m_nId = Common.InvalidID;

		// Token: 0x0400060C RID: 1548
		internal LHashSet<int> m_hsCrossRefs;

		// Token: 0x0400060D RID: 1549
		internal _IDataLocation m_location;

		// Token: 0x0400060E RID: 1550
		private readonly object _modifyingAccessLock = new object();

		// Token: 0x0400060F RID: 1551
		private readonly HashSet<int> _modifyingAccesses = new HashSet<int>();
	}
}
