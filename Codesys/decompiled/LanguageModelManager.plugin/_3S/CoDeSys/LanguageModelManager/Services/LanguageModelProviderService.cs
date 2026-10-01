using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000250 RID: 592
	public class LanguageModelProviderService : ILMProviderService2, ILMProviderService
	{
		// Token: 0x14000058 RID: 88
		// (add) Token: 0x060027A2 RID: 10146 RVA: 0x000637D4 File Offset: 0x000627D4
		// (remove) Token: 0x060027A3 RID: 10147 RVA: 0x0006380C File Offset: 0x0006280C
		public event EventHandler AfterLazyLibraryLoad;

		// Token: 0x14000059 RID: 89
		// (add) Token: 0x060027A4 RID: 10148 RVA: 0x00063844 File Offset: 0x00062844
		// (remove) Token: 0x060027A5 RID: 10149 RVA: 0x0006387C File Offset: 0x0006287C
		public event SetLibraryPreCompileContextCompletionPostProcessEventHandler SetLibraryPreCompileSetCompletionPostProcess;

		// Token: 0x060027A6 RID: 10150 RVA: 0x000638B1 File Offset: 0x000628B1
		public LanguageModelProviderService(LanguageModelManagerConsolidated lmm)
		{
			lmm.AfterLazyLibraryLoad += this.LanguageModelMgrOnAfterLazyLibraryLoad;
			lmm.SetLibraryPreCompileContextCompletionPostProcess += this.LanguageModelMgrOnSetLibraryPreCompileSetCompletionPostProcess;
		}

		// Token: 0x060027A7 RID: 10151 RVA: 0x000638DD File Offset: 0x000628DD
		private void LanguageModelMgrOnSetLibraryPreCompileSetCompletionPostProcess(object sender, SetLibraryPreCompileContextCompletionPostProcessEventArgs args)
		{
			SetLibraryPreCompileContextCompletionPostProcessEventHandler setLibraryPreCompileSetCompletionPostProcess = this.SetLibraryPreCompileSetCompletionPostProcess;
			if (setLibraryPreCompileSetCompletionPostProcess == null)
			{
				return;
			}
			setLibraryPreCompileSetCompletionPostProcess(sender, args);
		}

		// Token: 0x060027A8 RID: 10152 RVA: 0x000638F1 File Offset: 0x000628F1
		private void LanguageModelMgrOnAfterLazyLibraryLoad(object sender, EventArgs eventArgs)
		{
			EventHandler afterLazyLibraryLoad = this.AfterLazyLibraryLoad;
			if (afterLazyLibraryLoad == null)
			{
				return;
			}
			afterLazyLibraryLoad(sender, eventArgs);
		}

		// Token: 0x17000AFD RID: 2813
		// (get) Token: 0x060027A9 RID: 10153 RVA: 0x00063905 File Offset: 0x00062905
		public bool DelayedLoaderWorking
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoaderWorking;
			}
		}

		// Token: 0x17000AFE RID: 2814
		// (get) Token: 0x060027AA RID: 10154 RVA: 0x00063916 File Offset: 0x00062916
		public int PendingLibrariesInLoadQueue
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.PendingLibrariesInLoadQueue;
			}
		}

		// Token: 0x060027AB RID: 10155 RVA: 0x00063927 File Offset: 0x00062927
		public void EnqueueSetLibraryPreCompileSetFromArchive(Stream stream, Guid readerGuid, string stLibraryId, ISharedDataStorage sharedDataStorage, SetLibraryPreCompileContextCompletionEventHandler completionEventHandler, object callerData)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.EnqueueSetLibraryPreCompileContextFromArchive(stream, readerGuid, stLibraryId, sharedDataStorage, completionEventHandler, callerData);
		}

		// Token: 0x060027AC RID: 10156 RVA: 0x00063941 File Offset: 0x00062941
		public Guid GetApplicationGuidByName(string stName)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationGuidByName(stName);
		}

		// Token: 0x060027AD RID: 10157 RVA: 0x00063953 File Offset: 0x00062953
		public string GetApplicationNameByGuid(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(guidApplication);
		}

		// Token: 0x060027AE RID: 10158 RVA: 0x00063968 File Offset: 0x00062968
		public string GetApplicationNameByGuid(Guid guidApplication, EQueryApplicationNameFlags eFlags)
		{
			bool flag = EQueryApplicationNameFlags.SimulationMode == eFlags;
			bool flag2 = EQueryApplicationNameFlags.Unqualified == eFlags;
			if (flag)
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(guidApplication, flag);
			}
			if (flag2)
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationName(guidApplication, false);
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(guidApplication, false);
		}

		// Token: 0x060027AF RID: 10159 RVA: 0x000639BF File Offset: 0x000629BF
		public Guid GetParentApplicationGuid(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(guidApplication);
		}

		// Token: 0x060027B0 RID: 10160 RVA: 0x000639D6 File Offset: 0x000629D6
		public IEnumerable<Guid> GetSubApplicationGuids(Guid guidApplication, bool bRecursive)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetChildApplications(guidApplication, bRecursive);
		}

		// Token: 0x060027B1 RID: 10161 RVA: 0x000639F0 File Offset: 0x000629F0
		public bool IsExcludedFromBuild(int projectHandle, Guid objectGuid)
		{
			bool flag;
			return this.IsExcludedFromBuild(projectHandle, objectGuid, out flag);
		}

		// Token: 0x060027B2 RID: 10162 RVA: 0x000477C3 File Offset: 0x000467C3
		public bool IsExcludedFromBuild(int projectHandle, Guid objectGuid, out bool inherited)
		{
			return APEnvironmentFacade.Instance.IsExcludedFromBuild(projectHandle, objectGuid, out inherited);
		}

		// Token: 0x060027B3 RID: 10163 RVA: 0x00063A07 File Offset: 0x00062A07
		public void ProcessQueuedLibraryPreCompileSets(bool bWaitForCompletion)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.ProcessQueuedLibraryPreCompileContexts(bWaitForCompletion);
		}

		// Token: 0x060027B4 RID: 10164 RVA: 0x00063A19 File Offset: 0x00062A19
		public void ProcessQueuedLibraryPreCompileSets(IProgressCallback callback)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.ProcessQueuedLibraryPreCompileContexts(callback);
		}

		// Token: 0x060027B5 RID: 10165 RVA: 0x00063A2B File Offset: 0x00062A2B
		public void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.PutLanguageModel(lanmodprov, bShowSyntaxErrors);
		}

		// Token: 0x060027B6 RID: 10166 RVA: 0x00063A3E File Offset: 0x00062A3E
		public void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors, bool forceCompleteLanguageModel)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.PutLanguageModel(lanmodprov, bShowSyntaxErrors, forceCompleteLanguageModel);
		}

		// Token: 0x060027B7 RID: 10167 RVA: 0x00063A52 File Offset: 0x00062A52
		public void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.RemoveLanguageModelOfObject(nProjectHandle, objectGuid);
		}

		// Token: 0x060027B8 RID: 10168 RVA: 0x00063A65 File Offset: 0x00062A65
		public void RemoveLanguageModelOfObject(string libraryId, Guid objectGuid)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.RemoveLanguageModelOfObject(libraryId, objectGuid);
		}

		// Token: 0x060027B9 RID: 10169 RVA: 0x00063A78 File Offset: 0x00062A78
		public void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid, bool bShowPrecompileErrors)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.RemoveLanguageModelOfObject(nProjectHandle, objectGuid, bShowPrecompileErrors);
		}

		// Token: 0x060027BA RID: 10170 RVA: 0x00063A8C File Offset: 0x00062A8C
		public void RemoveLanguageModelOfProject(string stProjectId)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.RemoveLanguageModelOfProject(stProjectId);
		}

		// Token: 0x060027BB RID: 10171 RVA: 0x00063A9E File Offset: 0x00062A9E
		public ILMPreCompileSet SetLibraryPreCompileSetFromArchive(IArchiveReader reader, string stLibraryId)
		{
			return (ILMPreCompileSet)APEnvironmentFacade.Instance.LanguageModelMgr.SetLibraryPreCompileContextFromArchive(reader, stLibraryId);
		}

		// Token: 0x060027BC RID: 10172 RVA: 0x00063AB6 File Offset: 0x00062AB6
		public ILMPreCompileSet SetLibraryPreCompileSetFromArchive(IArchiveReader2 reader, string stLibraryId, ISharedDataStorage sharedDataStorage)
		{
			return (ILMPreCompileSet)APEnvironmentFacade.Instance.LanguageModelMgr.SetLibraryPreCompileContextFromArchive(reader, stLibraryId, sharedDataStorage);
		}

		// Token: 0x060027BD RID: 10173 RVA: 0x00063ACF File Offset: 0x00062ACF
		public IEnumerable<Guid> GetRelatedLanguageModel(Guid guidObject)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetRelatedObjects(guidObject);
		}
	}
}
