using System;
using System.Diagnostics;
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
	// Token: 0x02000140 RID: 320
	[TypeGuid("{99fa1ec1-fc21-4c93-9f1e-0accb45e8a84}")]
	[StorageVersion("3.3.0.0")]
	public class DirectVariableCrossRefTable : GenericObject2, _IDirectVariableCrossRefTable, IDirectVariableCrossRefTable2, IDirectVariableCrossRefTable, IDirectVariableCrossRefTableSerializable
	{
		// Token: 0x06001B18 RID: 6936 RVA: 0x0004D0B0 File Offset: 0x0004C0B0
		public void SetNumberOfTasks(int nTaskNum)
		{
			this.m_htTaskTables = new LDictionary<int, LDictionary<IProcessImageLocation, IProcessImageLocation>>();
			for (int i = 0; i < nTaskNum; i++)
			{
				this.m_htTaskTables[i] = new LDictionary<IProcessImageLocation, IProcessImageLocation>();
			}
		}

		// Token: 0x06001B19 RID: 6937 RVA: 0x0004D0E8 File Offset: 0x0004C0E8
		public void AddCrossReference(IDirectVariable dirvar, int nSignatureId, IAddressCodePosition codepos)
		{
			MySortedListWrapper mySortedListWrapper = null;
			if (!this.m_htDirVars.TryGetValue(dirvar, ref mySortedListWrapper))
			{
				mySortedListWrapper = new MySortedListWrapper();
				this.m_htDirVars[dirvar] = mySortedListWrapper;
			}
			AddressCrossReference addressCrossReference = new AddressCrossReference(codepos, nSignatureId);
			AddressCrossReference addressCrossReference2;
			if (!mySortedListWrapper.List.TryGetValue(addressCrossReference, ref addressCrossReference2))
			{
				mySortedListWrapper.List[addressCrossReference] = addressCrossReference;
				return;
			}
			addressCrossReference2.AddPosition(codepos);
		}

		// Token: 0x17000744 RID: 1860
		// (get) Token: 0x06001B1A RID: 6938 RVA: 0x0004D148 File Offset: 0x0004C148
		public IDirectVariable[] AllDirectVariables
		{
			get
			{
				IDirectVariable[] array = new IDirectVariable[this.m_htDirVars.Keys.Count];
				this.m_htDirVars.Keys.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x06001B1B RID: 6939 RVA: 0x0004D180 File Offset: 0x0004C180
		public IAddressCrossReference[] GetCrossReferencesOfDirectVariable(IDirectVariable dirvar)
		{
			MySortedListWrapper mySortedListWrapper = this.m_htDirVars[dirvar];
			if (mySortedListWrapper == null)
			{
				return Array.Empty<IAddressCrossReference>();
			}
			AddressCrossReference[] array = new AddressCrossReference[mySortedListWrapper.List.Count];
			mySortedListWrapper.List.Values.CopyTo(array, 0);
			return array;
		}

		// Token: 0x06001B1C RID: 6940 RVA: 0x0004D1CC File Offset: 0x0004C1CC
		public IProcessImageLocation[] GetProcessImageLocations(int nTaskId)
		{
			if (this.m_htTaskTables == null)
			{
				return Array.Empty<IProcessImageLocation>();
			}
			IProcessImageLocation[] array = new IProcessImageLocation[this.m_htTaskTables[nTaskId].Keys.Count];
			this.m_htTaskTables[nTaskId].Keys.CopyTo(array, 0);
			return array;
		}

		// Token: 0x06001B1D RID: 6941 RVA: 0x0004D21C File Offset: 0x0004C21C
		public void AddProcessImageLocation(IDataLocation datloc, int nSize, int nTaskId, DirectVariableLocation loc, AccessFlag access)
		{
			Debug.Assert(this.m_htTaskTables.Count > nTaskId);
			_IProcessImageLocation iprocessImageLocation = LanguageModelBuilder.Singleton.CreateProcessImageLocation(datloc, nSize, loc) as _IProcessImageLocation;
			IProcessImageLocation processImageLocation = null;
			if (this.m_htTaskTables[nTaskId].TryGetValue(iprocessImageLocation, ref processImageLocation))
			{
				iprocessImageLocation = (processImageLocation as _IProcessImageLocation);
				iprocessImageLocation.AddAccess(access);
				return;
			}
			iprocessImageLocation.AddAccess(access);
			this.m_htTaskTables[nTaskId][iprocessImageLocation] = iprocessImageLocation;
		}

		// Token: 0x040005AB RID: 1451
		[DefaultSerialization("table_1")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private LDictionary<IDirectVariable, MySortedListWrapper> m_htDirVars = new LDictionary<IDirectVariable, MySortedListWrapper>();

		// Token: 0x040005AC RID: 1452
		[Obfuscation(Feature = "rename")]
		private LDictionary<int, LDictionary<IProcessImageLocation, IProcessImageLocation>> m_htTaskTables;
	}
}
