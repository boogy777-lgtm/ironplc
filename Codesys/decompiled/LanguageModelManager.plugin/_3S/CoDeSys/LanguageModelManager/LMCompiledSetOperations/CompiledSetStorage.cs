using System;
using System.Collections.Generic;
using System.IO;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.Interfaces;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.LMCompiledSetOperations
{
	// Token: 0x020001B5 RID: 437
	public class CompiledSetStorage : ILMCompiledSetStorage
	{
		// Token: 0x1700083D RID: 2109
		// (get) Token: 0x06001F67 RID: 8039 RVA: 0x00056561 File Offset: 0x00055561
		public IDictionary<Guid, ILMCompiledApplicationSet> ReferenceResources
		{
			get
			{
				return this.m_htReferenceResources;
			}
		}

		// Token: 0x1700083E RID: 2110
		// (get) Token: 0x06001F68 RID: 8040 RVA: 0x00056569 File Offset: 0x00055569
		public IDictionary<Guid, ILMCompiledApplicationSet> CompiledResources
		{
			get
			{
				return this.m_htCompiledResources;
			}
		}

		// Token: 0x1700083F RID: 2111
		// (get) Token: 0x06001F69 RID: 8041 RVA: 0x00056571 File Offset: 0x00055571
		private IDictionary<Guid, ILMCompiledApplicationSet> CompiledResourcesWithStackoverflow
		{
			get
			{
				return this.m_htCompiledResourcesWithStackoverflow;
			}
		}

		// Token: 0x17000840 RID: 2112
		// (get) Token: 0x06001F6A RID: 8042 RVA: 0x00056579 File Offset: 0x00055579
		public IEnumerable<ILMCompiledApplicationSet> CompiledApplicationSets
		{
			get
			{
				return this.CompiledResources.Values;
			}
		}

		// Token: 0x17000841 RID: 2113
		// (get) Token: 0x06001F6B RID: 8043 RVA: 0x00056586 File Offset: 0x00055586
		public IEnumerable<ILMCompiledApplicationSet> DownloadedApplicationSets
		{
			get
			{
				return this.ReferenceResources.Values;
			}
		}

		// Token: 0x06001F6C RID: 8044 RVA: 0x00056593 File Offset: 0x00055593
		public void Clear()
		{
			this.CompiledResources.Clear();
			this.ReferenceResources.Clear();
			this.CompiledResourcesWithStackoverflow.Clear();
		}

		// Token: 0x06001F6D RID: 8045 RVA: 0x000565B8 File Offset: 0x000555B8
		public ILMCompiledApplicationSet GetCompiledApplicationSet(Guid guidApplication)
		{
			guidApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			ILMCompiledApplicationSet ilmcompiledApplicationSet = null;
			if (this.CompiledResources.TryGetValue(guidApplication, out ilmcompiledApplicationSet))
			{
				return ilmcompiledApplicationSet;
			}
			ilmcompiledApplicationSet = this.GetDownloadedApplicationSet(guidApplication);
			if (ilmcompiledApplicationSet != null)
			{
				this.CompiledResources[guidApplication] = ilmcompiledApplicationSet;
				return ilmcompiledApplicationSet;
			}
			return null;
		}

		// Token: 0x06001F6E RID: 8046 RVA: 0x0005660B File Offset: 0x0005560B
		public void SetCompiledApplicationSet(Guid guidApplication, ILMCompiledApplicationSet set)
		{
			guidApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			this.CompiledResources[guidApplication] = set;
		}

		// Token: 0x06001F6F RID: 8047 RVA: 0x00056631 File Offset: 0x00055631
		public void RemoveCompiledApplicationSet(Guid guidApplication)
		{
			if (this.CompiledResources.ContainsKey(guidApplication))
			{
				this.CompiledResources.Remove(guidApplication);
			}
			this.RemoveCompiledApplicationSetWithStackoverflow(guidApplication);
		}

		// Token: 0x06001F70 RID: 8048 RVA: 0x00056658 File Offset: 0x00055658
		public ILMCompiledApplicationSet GetDownloadedApplicationSet(Guid guidApplication)
		{
			guidApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			_IPreCompileContext ipreCompileContext = (_IPreCompileContext)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPreCompileSet(guidApplication);
			bool flag = ipreCompileContext != null && ipreCompileContext.SimulationMode;
			ILMCompiledApplicationSet ilmcompiledApplicationSet;
			if (this.ReferenceResources.TryGetValue(guidApplication, out ilmcompiledApplicationSet) && flag == ((_ICompileContext)ilmcompiledApplicationSet).SimulationMode)
			{
				return ilmcompiledApplicationSet;
			}
			this.ReferenceResources.Remove(guidApplication);
			if (!LoadAndSaveOptionsHelper.EnableBackgroundLoading)
			{
				this.LoadReferenceContextFromFile(guidApplication, flag);
			}
			this.ReferenceResources.TryGetValue(guidApplication, out ilmcompiledApplicationSet);
			return ilmcompiledApplicationSet;
		}

		// Token: 0x06001F71 RID: 8049 RVA: 0x000566F0 File Offset: 0x000556F0
		public ILMCompiledApplicationSet GetDownloadedApplicationSetSynchronLoad(Guid guidApplication)
		{
			guidApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			_IPreCompileContext ipreCompileContext = (_IPreCompileContext)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPreCompileSet(guidApplication);
			bool flag = ipreCompileContext != null && ipreCompileContext.SimulationMode;
			ILMCompiledApplicationSet ilmcompiledApplicationSet = null;
			if (this.ReferenceResources.TryGetValue(guidApplication, out ilmcompiledApplicationSet) && flag == ((_ICompileContext)ilmcompiledApplicationSet).SimulationMode)
			{
				return ilmcompiledApplicationSet;
			}
			this.ReferenceResources.Remove(guidApplication);
			this.LoadReferenceContextFromFile(guidApplication, flag);
			this.ReferenceResources.TryGetValue(guidApplication, out ilmcompiledApplicationSet);
			return ilmcompiledApplicationSet;
		}

		// Token: 0x06001F72 RID: 8050 RVA: 0x00056784 File Offset: 0x00055784
		internal ILMCompiledApplicationSet GetDownloadedApplicationSetSynchronLoad(Guid guidApplication, bool bSimulationMode)
		{
			guidApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			ILMCompiledApplicationSet ilmcompiledApplicationSet;
			if (this.ReferenceResources.TryGetValue(guidApplication, out ilmcompiledApplicationSet) && bSimulationMode == ((_ICompileContext)ilmcompiledApplicationSet).SimulationMode)
			{
				return ilmcompiledApplicationSet;
			}
			this.ReferenceResources.Remove(guidApplication);
			this.LoadReferenceContextFromFile(guidApplication, bSimulationMode);
			this.ReferenceResources.TryGetValue(guidApplication, out ilmcompiledApplicationSet);
			return ilmcompiledApplicationSet;
		}

		// Token: 0x06001F73 RID: 8051 RVA: 0x000567F0 File Offset: 0x000557F0
		public ILMCompiledApplicationSet GetDownloadedApplicationSetForBootApplicationSynchronLoad(Guid guidApplication)
		{
			string text = Path.ChangeExtension(APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationFileNameNew(guidApplication, false, false), ".bootinfo");
			if (!SideCarEntryHelper.ExistsFromPath(text))
			{
				return null;
			}
			this.LoadCompiledApplicationSetFromFileSync(guidApplication, text);
			return this.GetDownloadedApplicationSet(guidApplication);
		}

		// Token: 0x06001F74 RID: 8052 RVA: 0x00056834 File Offset: 0x00055834
		public void LoadDownloadedApplicationSetInBackground(Guid guidApplication)
		{
			_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPreCompileSet(guidApplication) as _IPreCompileContext;
			if (ipreCompileContext == null)
			{
				return;
			}
			string applicationName = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationName(guidApplication, ipreCompileContext.SimulationMode);
			string applicationFileNameNew = APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationFileNameNew(guidApplication, false, ipreCompileContext.SimulationMode);
			if (SideCarEntryHelper.ExistsFromPath(applicationFileNameNew))
			{
				ISideCarEntry wrapperFromPath = SideCarEntryHelper.GetWrapperFromPath(applicationFileNameNew);
				((DelayedLoader)APEnvironmentFacade.Instance.LanguageModelMgr.DelayedLoader).EnqueueRefContextItem(applicationFileNameNew, applicationName, wrapperFromPath);
			}
		}

		// Token: 0x06001F75 RID: 8053 RVA: 0x000568BF File Offset: 0x000558BF
		public void SetDownloadedApplicationSet(Guid guidApplication, ILMCompiledApplicationSet set, bool bAbortStorageOfPrevious)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.WaitForContextSavingThreadToFinish(guidApplication, bAbortStorageOfPrevious);
			guidApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			this.ReferenceResources[guidApplication] = set;
		}

		// Token: 0x06001F76 RID: 8054 RVA: 0x000568F6 File Offset: 0x000558F6
		public void RemoveDownloadedApplicationSet(Guid guidApplication)
		{
			if (this.ReferenceResources.ContainsKey(guidApplication))
			{
				this.ReferenceResources.Remove(guidApplication);
			}
		}

		// Token: 0x06001F77 RID: 8055 RVA: 0x00056914 File Offset: 0x00055914
		private void LoadReferenceContextFromFile(Guid guidApplication, bool bSimulation)
		{
			if (this.m_bReentranceBreak)
			{
				return;
			}
			this.m_bReentranceBreak = true;
			string applicationFileNameNew = APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationFileNameNew(guidApplication, false, bSimulation);
			this.LoadCompiledApplicationSetFromFileSync(guidApplication, applicationFileNameNew);
		}

		// Token: 0x06001F78 RID: 8056 RVA: 0x0005694C File Offset: 0x0005594C
		private void LoadCompiledApplicationSetFromFileSync(Guid guidApplication, string stPath)
		{
			IProgressCallback progressCallback = null;
			try
			{
				if (SideCarEntryHelper.ExistsFromPath(stPath))
				{
					progressCallback = APEnvironmentFacade.Instance.StartLengthyOperation();
					ProgressX._NotifyNextTask(progressCallback, true, Strings.LoadCompileContext, 0, null);
					ProgressX._NotifyTaskProgress(progressCallback, stPath);
					bool flag = false;
					try
					{
						flag = true;
						CompileContext compileContext;
						using (Stream stream = SideCarEntryHelper.OpenReadFromPath(stPath))
						{
							compileContext = LMCompiledSetPersistence.LoadFromStream<CompileContext>(stream);
						}
						if (compileContext != null)
						{
							compileContext.DataManager._MemorySettings = MemorySettingsHelperX._GetMemorySettings(APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(guidApplication), APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(guidApplication), guidApplication, compileContext.SimulationMode);
							this.SetDownloadedApplicationSet(guidApplication, compileContext, false);
							if (this.GetCompiledApplicationSet(guidApplication) == null)
							{
								this.SetCompiledApplicationSet(guidApplication, compileContext);
							}
						}
					}
					catch (Exception ex)
					{
						string stText = string.Format(Strings.CompileContextLoadError, stPath, ex.Message);
						APEnvironmentFacade.Instance.MessageStorage.AddMessage(CompileContextMessageCategory.Singleton, new CompileContextMessage(stText, Severity.Warning));
						if (flag)
						{
							SideCarEntryHelper.DeleteFromPath(stPath);
						}
					}
				}
			}
			finally
			{
				if (progressCallback != null)
				{
					progressCallback.Finish();
				}
				this.m_bReentranceBreak = false;
			}
		}

		// Token: 0x06001F79 RID: 8057 RVA: 0x00056A7C File Offset: 0x00055A7C
		public void SetCompiledApplicationSetWithStackoverflow(Guid guidApplication, ILMCompiledApplicationSet set)
		{
			guidApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			this.CompiledResourcesWithStackoverflow[guidApplication] = set;
		}

		// Token: 0x06001F7A RID: 8058 RVA: 0x00056AA4 File Offset: 0x00055AA4
		public ILMCompiledApplicationSet GetCompiledApplicationSetWithStackoverflow(Guid guidApplication)
		{
			guidApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			ILMCompiledApplicationSet result;
			if (this.CompiledResourcesWithStackoverflow.TryGetValue(guidApplication, out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x06001F7B RID: 8059 RVA: 0x00056ADB File Offset: 0x00055ADB
		public void RemoveCompiledApplicationSetWithStackoverflow(Guid guidApplication)
		{
			if (this.CompiledResourcesWithStackoverflow.ContainsKey(guidApplication))
			{
				this.CompiledResourcesWithStackoverflow.Remove(guidApplication);
			}
		}

		// Token: 0x04000625 RID: 1573
		private readonly LDictionary<Guid, ILMCompiledApplicationSet> m_htCompiledResources = new LDictionary<Guid, ILMCompiledApplicationSet>();

		// Token: 0x04000626 RID: 1574
		private readonly LDictionary<Guid, ILMCompiledApplicationSet> m_htReferenceResources = new LDictionary<Guid, ILMCompiledApplicationSet>();

		// Token: 0x04000627 RID: 1575
		private readonly LDictionary<Guid, ILMCompiledApplicationSet> m_htCompiledResourcesWithStackoverflow = new LDictionary<Guid, ILMCompiledApplicationSet>();

		// Token: 0x04000628 RID: 1576
		private bool m_bReentranceBreak;
	}
}
