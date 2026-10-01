using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0006;
using CODESYS.Parser;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelUtilities;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.ProjectInfoObject;
using _3S.CoDeSys.Simulation;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220
{
	// Token: 0x02000005 RID: 5
	internal sealed class APEnvironmentFacadeDesktop : IAPEnvironmentFacade
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000005 RID: 5 RVA: 0x00002080 File Offset: 0x00000280
		public bool InjectionCompleted
		{
			get
			{
				return APEnvironment.InjectionCompleted;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000006 RID: 6 RVA: 0x00002088 File Offset: 0x00000288
		public _ILanguageModelManagerConsolidated LanguageModelMgr
		{
			get
			{
				return APEnvironment.LanguageModelMgr;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000007 RID: 7 RVA: 0x00002090 File Offset: 0x00000290
		public ILMServiceProvider3 LMServiceProvider
		{
			get
			{
				return APEnvironment.LMServiceProvider;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002098 File Offset: 0x00000298
		public ILanguageModelUtilities2 LanguageModelUtilities
		{
			get
			{
				return APEnvironment.LanguageModelUtilities;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000009 RID: 9 RVA: 0x000020A0 File Offset: 0x000002A0
		public ICrossReferenceService4 CrossReferenceService
		{
			get
			{
				return APEnvironment.CrossReferenceService;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600000A RID: 10 RVA: 0x000020A8 File Offset: 0x000002A8
		public ILMCompileOptions3 CompileOptions
		{
			get
			{
				return APEnvironment.CompileOptions;
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000020B0 File Offset: 0x000002B0
		public ITargetSettings GetTargetSettingsById(IDeviceIdentification devId)
		{
			return APEnvironment.TargetSettingsMgr.Settings.GetTargetSettingsById(devId);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000020C4 File Offset: 0x000002C4
		public ITargetSettings GetSimulationTargetSettings(IDeviceIdentification devId, Guid deviceGuid)
		{
			ISimulationManager simulationManager = APEnvironment.SimulationManager;
			ISimulationManagerWithContext simulationManagerWithContext = simulationManager as ISimulationManagerWithContext;
			if (simulationManagerWithContext != null)
			{
				return this.GetTargetSettingsById(simulationManagerWithContext.GetSimulationDeviceIdentification(devId, this.PrimaryProjectHandle, deviceGuid));
			}
			return this.GetTargetSettingsById(simulationManager.SimulationDeviceIdentification);
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600000D RID: 13 RVA: 0x00002104 File Offset: 0x00000304
		public IMessageStorage MessageStorage
		{
			get
			{
				return APEnvironment.MessageStorage;
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x0000210C File Offset: 0x0000030C
		public bool IsWarningMessageDisabled(MessageId id)
		{
			return APEnvironment.WarningHelper.IsWarningMessageDisabled(id);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x0000211C File Offset: 0x0000031C
		public bool IsWarningAsError(MessageId id)
		{
			_IWarningHelper2 iwarningHelper = APEnvironment.WarningHelper as _IWarningHelper2;
			return iwarningHelper != null && iwarningHelper.IsWarningAsError(id);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002140 File Offset: 0x00000340
		public IProgressCallback StartLengthyOperation()
		{
			return APEnvironment.Engine.StartLengthyOperation();
		}

		// Token: 0x06000011 RID: 17 RVA: 0x0000214C File Offset: 0x0000034C
		public bool ExistProject(int iProjectHandle)
		{
			return APEnvironment.Engine.Projects.GetProjectByHandle(iProjectHandle) != null;
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000012 RID: 18 RVA: 0x00002164 File Offset: 0x00000364
		public bool ExistsPrimaryProject
		{
			get
			{
				return APEnvironment.Engine.Projects.PrimaryProject != null;
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002178 File Offset: 0x00000378
		public bool IsPrimaryProject(int iProjectHandle)
		{
			IProject projectByHandle = APEnvironment.Engine.Projects.GetProjectByHandle(iProjectHandle);
			return projectByHandle != null && projectByHandle.Primary;
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000021A4 File Offset: 0x000003A4
		public int PrimaryProjectHandle
		{
			get
			{
				return APEnvironment.Engine.Projects.PrimaryProject.Handle;
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000021BC File Offset: 0x000003BC
		public bool IsLibrary(int iProjectHandle)
		{
			return APEnvironment.Engine.Projects.GetProjectByHandle(iProjectHandle).Library;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000021D4 File Offset: 0x000003D4
		public bool IsPrimaryProjectALibrary()
		{
			return string.Equals(Path.GetExtension(APEnvironment.Engine.Projects.PrimaryProject.Path), ".library", StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000021FC File Offset: 0x000003FC
		public bool IsPrimaryProjectAnInterfaceLibrary()
		{
			bool flag = string.Equals(Path.GetExtension(APEnvironment.Engine.Projects.PrimaryProject.Path), ".library", StringComparison.OrdinalIgnoreCase);
			Guid objectGuid = new Guid("{11C0FC3A-9BCF-4dd8-AC38-EFB93363E521}");
			if (flag && this.ExistsObject(this.PrimaryProjectHandle, objectGuid))
			{
				IProjectInfoObject projectInfoObject = (IProjectInfoObject)APEnvironment.ObjectMgr.GetObjectToRead(this.PrimaryProjectHandle, objectGuid).Object;
				object value;
				return projectInfoObject.ContainsValue("IsInterfaceLibrary") && (value = projectInfoObject.GetValue("IsInterfaceLibrary")) is bool && (bool)value;
			}
			return false;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002294 File Offset: 0x00000494
		public bool IsLibraryWithErrorSummarization(string stLibraryId)
		{
			IProjectInfoObject projectInfoObject = this.\u0001(stLibraryId);
			object value;
			return projectInfoObject != null && (projectInfoObject.ContainsValue("SummarizeErrors") && (value = projectInfoObject.GetValue("SummarizeErrors")) is bool && (bool)value);
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000022DC File Offset: 0x000004DC
		public bool ExistsObject(int nProjectHandle, Guid objectGuid)
		{
			return APEnvironment.ObjectMgr.ExistsObject(nProjectHandle, objectGuid);
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001A RID: 26 RVA: 0x000022EC File Offset: 0x000004EC
		public Guid ActiveApplicationGuid
		{
			get
			{
				Guid result;
				try
				{
					IProject primaryProject = APEnvironment.Engine.Projects.PrimaryProject;
					result = ((primaryProject != null) ? primaryProject.ActiveApplication : Guid.Empty);
				}
				catch
				{
					result = Guid.Empty;
				}
				return result;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600001B RID: 27 RVA: 0x00002338 File Offset: 0x00000538
		public Guid ProjectInfoObjectGuid
		{
			get
			{
				Guid[] allObjects = APEnvironment.ObjectMgr.GetAllObjects(this.PrimaryProjectHandle);
				Guid result = Guid.Empty;
				foreach (Guid guid in allObjects)
				{
					if (\u0006.\u0001.\u0001(APEnvironment.ObjectMgr.GetMetaObjectStub(this.PrimaryProjectHandle, guid).ObjectType, "_3S.CoDeSys.WorkspaceObject.IWorkspaceObject"))
					{
						result = guid;
						break;
					}
				}
				return result;
			}
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000239C File Offset: 0x0000059C
		public Guid GetPoolLibMan(int nProjectHandle)
		{
			foreach (Guid guid in APEnvironment.ObjectMgr.GetAllObjects(nProjectHandle))
			{
				IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, guid);
				if (typeof(ILibManObject).IsAssignableFrom(metaObjectStub.ObjectType) && metaObjectStub.ParentObjectGuid == Guid.Empty)
				{
					return guid;
				}
			}
			return Guid.Empty;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002408 File Offset: 0x00000608
		public ICodegenerator CreateCodegenerator(Guid typeGuid)
		{
			return APEnvironment.\u0001(typeGuid);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002410 File Offset: 0x00000610
		private IProjectInfoObject \u0001(string \u0002)
		{
			try
			{
				Guid objectGuid = new Guid("{11C0FC3A-9BCF-4dd8-AC38-EFB93363E521}");
				int projectHandle = this.LanguageModelMgr.LibList.GetProjectHandle(\u0002);
				if (this.ExistsObject(projectHandle, objectGuid))
				{
					return APEnvironment.ObjectMgr.GetObjectToRead(projectHandle, objectGuid).Object as IProjectInfoObject;
				}
			}
			catch
			{
				return null;
			}
			return null;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002478 File Offset: 0x00000678
		public IEnumerable<Guid> GetOpenEditorSignatures()
		{
			LHashSet<Guid> lhashSet = new LHashSet<Guid>();
			try
			{
				if (APEnvironment.Engine.Frame != null)
				{
					Enumerable.AddRange<Guid>(lhashSet, APEnvironment.Engine.EditorManager.GetEditors().Where(new Func<IEditor, bool>(APEnvironmentFacadeDesktop.<>c.<>9.\u0001)).Select(new Func<IEditor, Guid>(APEnvironmentFacadeDesktop.<>c.<>9.\u0001)));
				}
			}
			catch
			{
			}
			return lhashSet;
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000020 RID: 32 RVA: 0x0000250C File Offset: 0x0000070C
		public IOEMCustomization OEMCustomization
		{
			get
			{
				return APEnvironment.Engine.OEMCustomization;
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002518 File Offset: 0x00000718
		public _IPreCompileContext GetLibraryContext(int nProjectHandle)
		{
			IProject projectByHandle = APEnvironment.Engine.Projects.GetProjectByHandle(nProjectHandle);
			if (projectByHandle == null)
			{
				return null;
			}
			return this.LanguageModelMgr.LibList.GetLibraryContext(projectByHandle.Id);
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000022 RID: 34 RVA: 0x00002554 File Offset: 0x00000754
		public _ICompilerVersionSettings CompilerVersionSettings
		{
			get
			{
				return APEnvironment.CompilerVersionSettings;
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000255C File Offset: 0x0000075C
		public IVarConfigCodeGenerator TryCreateVarConfigCodeGenerator(Guid typeGuid)
		{
			return APEnvironment.\u0001(typeGuid);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002564 File Offset: 0x00000764
		public IAddressCalculator TryCreateAddressCalculator(Guid typeGuid)
		{
			return APEnvironment.\u0001(typeGuid);
		}

		// Token: 0x06000025 RID: 37 RVA: 0x0000256C File Offset: 0x0000076C
		public IMemoryAllocationCallback TryCreateMemoryAllocationCallback(Guid typeGuid)
		{
			return APEnvironment.\u0001(typeGuid);
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000026 RID: 38 RVA: 0x00002574 File Offset: 0x00000774
		public IDisplayNameParser DisplayNameParser
		{
			get
			{
				return APEnvironment.DisplayNameParser;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000027 RID: 39 RVA: 0x0000257C File Offset: 0x0000077C
		public IContainerLibraryChecker ContainerLibraryCheckerOrNull
		{
			get
			{
				return APEnvironment.ContainerLibraryCheckerOrNull;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000028 RID: 40 RVA: 0x00002584 File Offset: 0x00000784
		public INamespaceConflictChecker NamespaceConflictCheckerOrNull
		{
			get
			{
				return APEnvironment.NamespaceConflictCheckerOrNull;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000029 RID: 41 RVA: 0x0000258C File Offset: 0x0000078C
		public ICheckAllPoolObjectsConfigurationProvider CheckAllPoolObjectsConfigurationProviderOrNull
		{
			get
			{
				return APEnvironment.CheckAllPoolObjectsConfigurationProviderOrNull;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600002A RID: 42 RVA: 0x00002594 File Offset: 0x00000794
		public IMemoryStatisticsOutputProvider MemoryStatisticsOutputProviderOrNull
		{
			get
			{
				return APEnvironment.MemoryStatisticsOutputProviderOrNull;
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x0000259C File Offset: 0x0000079C
		public ILanguageModelProviderWithDependencies GetLanguageModelProviderWithDependencies(ISourcePosition sourcepos)
		{
			if (this.ExistsObject(sourcepos.ProjectHandle, sourcepos.ObjectGuid))
			{
				return APEnvironment.ObjectMgr.GetObjectToRead(sourcepos.ProjectHandle, sourcepos.ObjectGuid).Object as ILanguageModelProviderWithDependencies;
			}
			return null;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000025D4 File Offset: 0x000007D4
		public IEnumerable<IExplicitExpressionAtSourcePositionProvider> GetAllExplicitExpressionAtSourcePositionProviders()
		{
			return APEnvironment.ExplicitExpressionAtSourcePositionProviders;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x000025DC File Offset: 0x000007DC
		public bool GetOnlineApplicationPersistentInfo(Guid gdApplication, out uint uiCRC, out uint uiLength)
		{
			uiCRC = 0U;
			uiLength = 0U;
			IOnlineApplication4 onlineApplication = APEnvironment.OnlineMgr.GetApplication(gdApplication) as IOnlineApplication4;
			if (onlineApplication != null)
			{
				onlineApplication.GetPersistentInfo(out uiCRC, out uiLength);
				return true;
			}
			return false;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002610 File Offset: 0x00000810
		public PromptResult Prompt(string stMessage, PromptChoice choice, PromptResult defaultResult, string stMessageKey, params object[] messageArguments)
		{
			return APEnvironment.MessageService.Prompt(stMessage, choice, defaultResult, stMessageKey, messageArguments);
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600002F RID: 47 RVA: 0x00002624 File Offset: 0x00000824
		public string LibraryDevelopmentOptionsCompilerDefinesToUse
		{
			get
			{
				return APEnvironment.LibraryDevelopmentOptions.CompilerDefinesToUse;
			}
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002630 File Offset: 0x00000830
		public IArchiveReader CreateNewBinaryArchiveReader()
		{
			return APEnvironment.\u0001();
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002638 File Offset: 0x00000838
		public IArchiveReader CreateEncryptedBinaryArchiveReader()
		{
			return APEnvironment.\u0002();
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002640 File Offset: 0x00000840
		public IArchiveReader CreateNewEncryptedBinaryArchiveReader()
		{
			return APEnvironment.\u0002();
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002648 File Offset: 0x00000848
		public IArchiveWriter CreateNewLowMemoryFootprintBinaryArchiveWriter()
		{
			return APEnvironment.\u0001();
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002650 File Offset: 0x00000850
		public ISharedDataStorage GetSharedDataStorage(int nProjectHandle)
		{
			return APEnvironment.ObjectMgr.GetSharedDataStorage(nProjectHandle);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002660 File Offset: 0x00000860
		public Stream GetParseTreeStreamOfCompiledLibraryPOU(int nProjectHandle, Guid gdObject)
		{
			return APEnvironment.ParseTreeStreamProvider.GetParseTreeStreamOfCompiledLibraryPOU(nProjectHandle, gdObject);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002670 File Offset: 0x00000870
		public IPositionTextProvider GetPositionTextProvider(int nProjectHandle, Guid objectGuid)
		{
			try
			{
				if (this.ExistsObject(nProjectHandle, objectGuid))
				{
					return new ObjectRelatedPositionTextProvider(APEnvironment.ObjectMgr.GetObjectToRead(nProjectHandle, objectGuid).Object);
				}
			}
			catch
			{
			}
			return null;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000026B8 File Offset: 0x000008B8
		public IOptionKey CreateSubKey(OptionRoot root, string stSubKey)
		{
			return APEnvironment.OptionStorage.GetRootKey(root).CreateSubKey(stSubKey);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x000026CC File Offset: 0x000008CC
		public string GetObjectName(int nProjectHandle, Guid objectGuid)
		{
			return APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, objectGuid).Name;
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000039 RID: 57 RVA: 0x000026E0 File Offset: 0x000008E0
		public _ICheckerThread2 PrecompileChecker { get; } = new PrecompileChecksWindows();

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600003A RID: 58 RVA: 0x000026E8 File Offset: 0x000008E8
		public ILicensedSoftwareMetricInformationProvider LicensedSoftwareMetricInformationProvider
		{
			get
			{
				return APEnvironment.LicensedSoftwareMetricInformationProvider;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600003B RID: 59 RVA: 0x000026F0 File Offset: 0x000008F0
		public IProjectSideCarService ProjectSideCarService
		{
			get
			{
				return APEnvironment.ProjectSideCarService;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600003C RID: 60 RVA: 0x000026F8 File Offset: 0x000008F8
		public bool DebugFlagSet
		{
			get
			{
				return APEnvironment.Engine.CommandLineManager.HasSwitch("debug");
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002710 File Offset: 0x00000910
		public IParserService ParserService
		{
			get
			{
				return APEnvironment.ScannerParserProvider.ParserServiceToUseInternal();
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600003E RID: 62 RVA: 0x0000271C File Offset: 0x0000091C
		public IScannerService ScannerService
		{
			get
			{
				return APEnvironment.ScannerParserProvider.ScannerServiceToUseInternal();
			}
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002728 File Offset: 0x00000928
		public IScannerService GetScannerService(Version version)
		{
			return APEnvironment.ScannerParserProvider.GetScannerService(version);
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00002738 File Offset: 0x00000938
		public IParserService GetParserService(Version version)
		{
			return APEnvironment.ScannerParserProvider.GetParserService(version);
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000041 RID: 65 RVA: 0x00002748 File Offset: 0x00000948
		public ITaskStackSizeProvider TaskStackSizeProvider
		{
			get
			{
				return APEnvironment.TaskStackSizeProvider;
			}
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002750 File Offset: 0x00000950
		public IEmbeddedLanguageService[] GetEmbeddedLanguageServices()
		{
			return APEnvironment.EmbeddedLanguageServices.ToArray<IEmbeddedLanguageService>();
		}

		// Token: 0x04000002 RID: 2
		[CompilerGenerated]
		private readonly _ICheckerThread2 \u0001;
	}
}
