using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000244 RID: 580
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "grandfather clause (Bestandsschutz)")]
	public class CommandService : ILMCommandService4, ILMCommandService3, ILMCommandService2, ILMCommandService, ILMCachingService, ILMSaveCompatibleLibraryService
	{
		// Token: 0x1400003A RID: 58
		// (add) Token: 0x0600267F RID: 9855 RVA: 0x000604A4 File Offset: 0x0005F4A4
		// (remove) Token: 0x06002680 RID: 9856 RVA: 0x000604DC File Offset: 0x0005F4DC
		public event EventHandler AfterClearAll;

		// Token: 0x1400003B RID: 59
		// (add) Token: 0x06002681 RID: 9857 RVA: 0x00060514 File Offset: 0x0005F514
		// (remove) Token: 0x06002682 RID: 9858 RVA: 0x0006054C File Offset: 0x0005F54C
		public event EventHandler BeforeClearAll;

		// Token: 0x06002683 RID: 9859 RVA: 0x00060581 File Offset: 0x0005F581
		public CommandService(LanguageModelManagerConsolidated lmm)
		{
			lmm.BeforeClearAll += this.LanguageModelMgrOnBeforeClearAll;
			lmm.AfterClearAll += this.LanguageModelMgrOnAfterClearAll;
		}

		// Token: 0x06002684 RID: 9860 RVA: 0x000605AD File Offset: 0x0005F5AD
		private void LanguageModelMgrOnBeforeClearAll(object sender, EventArgs eventArgs)
		{
			EventHandler beforeClearAll = this.BeforeClearAll;
			if (beforeClearAll == null)
			{
				return;
			}
			beforeClearAll(sender, eventArgs);
		}

		// Token: 0x06002685 RID: 9861 RVA: 0x000605C1 File Offset: 0x0005F5C1
		private void LanguageModelMgrOnAfterClearAll(object sender, EventArgs eventArgs)
		{
			EventHandler afterClearAll = this.AfterClearAll;
			if (afterClearAll == null)
			{
				return;
			}
			afterClearAll(sender, eventArgs);
		}

		// Token: 0x06002686 RID: 9862 RVA: 0x000605D8 File Offset: 0x0005F5D8
		public bool CheckAllApplicationObjects(Guid guidApplication)
		{
			CompilerProxy.GetCheckerThread().Disable();
			IProgressCallback progressCallback = null;
			_ICompileContext value = APEnvironmentFacade.Instance.LanguageModelMgr[guidApplication];
			bool result;
			try
			{
				APEnvironmentFacade.Instance.LanguageModelMgr[guidApplication] = null;
				progressCallback = APEnvironmentFacade.Instance.StartLengthyOperation();
				ProgressX._NotifyNextTask(progressCallback, true, Strings.Build, 0, null);
				result = CompilerProxy.Compile(guidApplication, progressCallback, true, false);
			}
			finally
			{
				if (progressCallback != null)
				{
					progressCallback.Finish();
				}
				APEnvironmentFacade.Instance.LanguageModelMgr[guidApplication] = value;
				CompilerProxy.GetCheckerThread().Enable();
				CompilerProxy.GetCheckerThread().TryStart();
			}
			return result;
		}

		// Token: 0x06002687 RID: 9863 RVA: 0x00060678 File Offset: 0x0005F678
		public bool CheckAndLoadBootInfo(Guid guidApplication, Guid guidCode, Guid guidData)
		{
			string path = Path.ChangeExtension(APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationFileNameNew(guidApplication, false, false), ".bootinfo_guids");
			if (!SideCarEntryHelper.ExistsFromPath(path))
			{
				return false;
			}
			Stream stream = SideCarEntryHelper.OpenReadFromPath(path);
			byte[] array = new byte[16];
			byte[] array2 = new byte[16];
			stream.Read(array, 0, array.Length);
			stream.Read(array2, 0, array2.Length);
			stream.Close();
			Guid b = new Guid(array);
			Guid b2 = new Guid(array2);
			return guidCode == b && guidData == b2 && APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSetForBootApplicationSynchronLoad(guidApplication) != null;
		}

		// Token: 0x06002688 RID: 9864 RVA: 0x0006071C File Offset: 0x0005F71C
		public IEnumerable<IMessage> CheckInterfaceLibraryCompatibility(ILMPreCompileSet pccOlderLibrary, ILMPreCompileSet pccNewerLibrary)
		{
			return LibraryCompatibilityCheck.CheckLibrary(pccOlderLibrary as IPreCompileContext, pccNewerLibrary as IPreCompileContext, true);
		}

		// Token: 0x06002689 RID: 9865 RVA: 0x00060730 File Offset: 0x0005F730
		public IEnumerable<IMessage> CheckLibraryCompatibility(ILMPreCompileSet pccOlderLibrary, ILMPreCompileSet pccNewerLibrary, bool bInterfaceLibrary)
		{
			return LibraryCompatibilityCheck.CheckLibrary(pccOlderLibrary as IPreCompileContext, pccNewerLibrary as IPreCompileContext, bInterfaceLibrary);
		}

		// Token: 0x0600268A RID: 9866 RVA: 0x00060744 File Offset: 0x0005F744
		public void ClearAll()
		{
			this.ClearAll(false);
		}

		// Token: 0x0600268B RID: 9867 RVA: 0x0006074D File Offset: 0x0005F74D
		public void ClearAll(bool bAtProjectClose)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.ClearAll(bAtProjectClose);
		}

		// Token: 0x0600268C RID: 9868 RVA: 0x0006075F File Offset: 0x0005F75F
		public void ClearDownloadInfo(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.ClearDownloadContext(guidApplication);
		}

		// Token: 0x0600268D RID: 9869 RVA: 0x00060771 File Offset: 0x0005F771
		public bool Compile(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.Compile(guidApplication);
		}

		// Token: 0x0600268E RID: 9870 RVA: 0x00060783 File Offset: 0x0005F783
		public void CompileAll()
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.CompileAll();
		}

		// Token: 0x0600268F RID: 9871 RVA: 0x00060771 File Offset: 0x0005F771
		public bool CompileAndLocate(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.Compile(guidApplication);
		}

		// Token: 0x06002690 RID: 9872 RVA: 0x00060794 File Offset: 0x0005F794
		public void CreateBootDuplicate(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.CreateBootDuplicate(guidApplication);
		}

		// Token: 0x06002691 RID: 9873 RVA: 0x000607A6 File Offset: 0x0005F7A6
		public void ForceRebuildAll(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.ForceRebuildAll(guidApplication);
		}

		// Token: 0x06002692 RID: 9874 RVA: 0x000607B8 File Offset: 0x0005F7B8
		public bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GenerateCode(guidApplication, bOnlineChange, bKeepCompileInformation);
		}

		// Token: 0x06002693 RID: 9875 RVA: 0x000607CC File Offset: 0x0005F7CC
		public bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation, out IMessage[] errors, out IMessage[] warnings)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GenerateCode(guidApplication, bOnlineChange, bKeepCompileInformation, out errors, out warnings);
		}

		// Token: 0x06002694 RID: 9876 RVA: 0x000607E4 File Offset: 0x0005F7E4
		public bool GenerateOnlineChangeCode(Guid guidApplication, bool bKeepCompileInformation, out IOnlineChangeDetails ocd)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GenerateOnlineChangeCode(guidApplication, bKeepCompileInformation, out ocd);
		}

		// Token: 0x06002695 RID: 9877 RVA: 0x000607F8 File Offset: 0x0005F7F8
		public void OnSaveProjectAs(string stOldProjectPath, string stNewProjectPath)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.OnSaveProjectAs(stOldProjectPath, stNewProjectPath);
		}

		// Token: 0x06002696 RID: 9878 RVA: 0x0006080B File Offset: 0x0005F80B
		public void SavePreCompileSetToArchive(ILMPreCompileSet precom, IArchiveWriter writer)
		{
			(precom as PreCompileContext).SaveToArchive(writer);
		}

		// Token: 0x06002697 RID: 9879 RVA: 0x00060819 File Offset: 0x0005F819
		public void SavePreCompileSetToArchive(ILMPreCompileSet precom, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, Profile profile, IArchiveReporter reporter)
		{
			(precom as PreCompileContext).SaveToArchive(writer, sharedDataStorage, profile, reporter, PreCompileSetArchiveStorageFormat.ClassicalFormat);
		}

		// Token: 0x06002698 RID: 9880 RVA: 0x0006082D File Offset: 0x0005F82D
		public void SavePreCompileSetToArchive(ILMPreCompileSet precom, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, Profile profile, IArchiveReporter reporter, PreCompileSetArchiveStorageFormat archiveStorageFormat)
		{
			(precom as PreCompileContext).SaveToArchive(writer, sharedDataStorage, profile, reporter, archiveStorageFormat);
		}

		// Token: 0x06002699 RID: 9881 RVA: 0x00060842 File Offset: 0x0005F842
		public void SaveVersionedPrecompileSetToArchive(ILMPreCompileSet precom, Version v, IArchiveAuxiliaryWriter auxiliaryWriter, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, IArchiveReporter reporter)
		{
			((PreCompileContext)precom).SaveVersionedPrecompileSetToArchive(v, auxiliaryWriter, writer, sharedDataStorage, reporter);
		}

		// Token: 0x0600269A RID: 9882 RVA: 0x00060857 File Offset: 0x0005F857
		public void SaveParseTreeToArchive(ILMPreCompileSet precom, ICompiledPOU cpou, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage)
		{
			POUSaver.SaveParseTreeToArchive(cpou, writer, sharedDataStorage);
		}

		// Token: 0x0600269B RID: 9883 RVA: 0x00060862 File Offset: 0x0005F862
		public void SaveToProject(int nProjectHandle)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.SaveToProject(nProjectHandle);
		}

		// Token: 0x0600269C RID: 9884 RVA: 0x00060874 File Offset: 0x0005F874
		public void SimulationModeChanged(Guid guidDevice)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.SimulationModeChanged(guidDevice);
		}

		// Token: 0x0600269D RID: 9885 RVA: 0x00060886 File Offset: 0x0005F886
		public void SimulationModeChanged(Guid guidDevice, bool newSimulationMode)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.SimulationModeChanged(guidDevice, newSimulationMode);
		}

		// Token: 0x0600269E RID: 9886 RVA: 0x00060899 File Offset: 0x0005F899
		public void UpdateDownloadInfoAsync(Guid guidApplication)
		{
			this.UpdateDownloadInfoAsync(guidApplication, false);
		}

		// Token: 0x0600269F RID: 9887 RVA: 0x000608A3 File Offset: 0x0005F8A3
		public void UpdateDownloadInfoAsync(Guid guidApplication, bool bCreateBootDuplicate)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.UpdateDownloadContext(guidApplication, false, bCreateBootDuplicate);
		}

		// Token: 0x060026A0 RID: 9888 RVA: 0x000608B7 File Offset: 0x0005F8B7
		public void UpdateDownloadInfoSync(Guid guidApplication)
		{
			this.UpdateDownloadInfoSync(guidApplication, false);
		}

		// Token: 0x060026A1 RID: 9889 RVA: 0x000608C1 File Offset: 0x0005F8C1
		public void UpdateDownloadInfoSync(Guid guidApplication, bool bCreateBootDuplicate)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.UpdateDownloadContext(guidApplication, true, bCreateBootDuplicate);
		}

		// Token: 0x060026A2 RID: 9890 RVA: 0x000608D5 File Offset: 0x0005F8D5
		public void UpdateDownloadInfoInMemoryOnly(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.UpdateDownloadContextWithoutWriteContext(guidApplication);
		}

		// Token: 0x060026A3 RID: 9891 RVA: 0x000608E7 File Offset: 0x0005F8E7
		public string GetDownloadInfoFileName(Guid guidApplication, EQueryDownloadInfoFileNameFlags eFlags)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationFileNameNew(guidApplication, (EQueryDownloadInfoFileNameFlags.Precompile & eFlags) > EQueryDownloadInfoFileNameFlags.Default, (EQueryDownloadInfoFileNameFlags.Simulation & eFlags) > EQueryDownloadInfoFileNameFlags.Default);
		}

		// Token: 0x060026A4 RID: 9892 RVA: 0x00060905 File Offset: 0x0005F905
		public bool IsAsyncUpdateDownloadInfoInProgress(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsAsyncUpdateDownloadInfoInProgress(guidApplication);
		}

		// Token: 0x060026A5 RID: 9893 RVA: 0x00060917 File Offset: 0x0005F917
		public void WaitForAsyncUpdateDownloadInfoCompleted(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LanguageModelMgr.WaitForContextSavingThreadToFinish(guidApplication);
		}

		// Token: 0x060026A6 RID: 9894 RVA: 0x0006092C File Offset: 0x0005F92C
		public bool LoadCachedLanguageModel(Stream cache)
		{
			_ICheckerThread checkerThread = CompilerProxy.GetCheckerThread();
			try
			{
				LanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
				ILMSerializationService serializationService_OrNull = CompilerProxy.SerializationService_OrNull;
				checkerThread.Disable();
				BinaryReader binaryReader = new BinaryReader(cache);
				GreenTreeFactory singleton = GreenTreeFactory.Singleton;
				RedTreeFactory singleton2 = RedTreeFactory.Singleton;
				List<_IExprement> list = new List<_IExprement>();
				GreenTreeTables tables = GreenTreeContext.Singleton.Tables;
				serializationService_OrNull.DeSerializeExprementTable(binaryReader, singleton, list, tables);
				int num = binaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					_IPreCompileContext2 ipreCompileContext = serializationService_OrNull.DeSerializePrecompileContext(binaryReader, singleton2, singleton, list);
					ipreCompileContext.Dirty = true;
					languageModelMgr._SetPrecompileContext(ipreCompileContext);
				}
				serializationService_OrNull.DeSerializeRelatedObjectTable(binaryReader, languageModelMgr.RelatedObjectTable);
				languageModelMgr.ApplicationDeviceTable.Clear();
				serializationService_OrNull.DeSerializeApplicationDeviceTable(binaryReader, (_IApplicationDeviceTable2)languageModelMgr.ApplicationDeviceTable);
				ILMSerializationService2 ilmserializationService = serializationService_OrNull as ILMSerializationService2;
				if (ilmserializationService != null)
				{
					ilmserializationService.DeSerializeTextualPrecompileCrossReferences(binaryReader, languageModelMgr.PCCRVariables);
					ilmserializationService.DeSerializeTextualPrecompileCrossReferences(binaryReader, languageModelMgr.PCCRCalls);
					ilmserializationService.DeSerializeTextualPrecompileCrossReferences(binaryReader, languageModelMgr.PCCRDirVars);
				}
			}
			finally
			{
				checkerThread.Enable();
			}
			return true;
		}

		// Token: 0x060026A7 RID: 9895 RVA: 0x00060A40 File Offset: 0x0005FA40
		public void StoreCachedLanguageModel(Stream cache)
		{
			LanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			IPreCompileContext[] array = languageModelMgr.PrecompileContexts.Concat(new _IPreCompileContext[]
			{
				languageModelMgr.Pool
			}).ToArray<IPreCompileContext>();
			ILMSerializationService serializationService_OrNull = CompilerProxy.SerializationService_OrNull;
			BinaryWriter binaryWriter = new BinaryWriter(cache);
			GreenTreeTables tables = GreenTreeContext.Singleton.Tables;
			Dictionary<_IExprement, int> dictionary = new Dictionary<_IExprement, int>();
			serializationService_OrNull.SerializeTable(binaryWriter, tables, dictionary);
			binaryWriter.Write(array.Length);
			foreach (IPreCompileContext preCompileContext in array)
			{
				serializationService_OrNull.SerializePrecompileContext(binaryWriter, (_IPreCompileContext2)preCompileContext, dictionary);
			}
			serializationService_OrNull.SerializeRelatedObjectTable(binaryWriter, languageModelMgr.RelatedObjectTable);
			serializationService_OrNull.SerializeApplicationDeviceTable(binaryWriter, (_IApplicationDeviceTable2)languageModelMgr.ApplicationDeviceTable);
			ILMSerializationService2 ilmserializationService = serializationService_OrNull as ILMSerializationService2;
			if (ilmserializationService != null)
			{
				ilmserializationService.SerializeTextualPrecompileCrossReferences(binaryWriter, languageModelMgr.m_pccrVariables.AllAccessesSerializable());
				ilmserializationService.SerializeTextualPrecompileCrossReferences(binaryWriter, languageModelMgr.m_pccrCalls.AllAccessesSerializable());
				ilmserializationService.SerializeTextualPrecompileCrossReferences(binaryWriter, languageModelMgr.m_pccrDirVars.AllAccessesSerializable());
			}
		}
	}
}
