using System;
using System.Collections.Generic;
using System.IO;
using CODESYS.Parser;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelUtilities;

namespace _3S.CoDeSys.Compiler35220
{
	// Token: 0x02000007 RID: 7
	public interface IAPEnvironmentFacade
	{
		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000048 RID: 72
		bool InjectionCompleted { get; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000049 RID: 73
		_ILanguageModelManagerConsolidated LanguageModelMgr { get; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600004A RID: 74
		ILMServiceProvider3 LMServiceProvider { get; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600004B RID: 75
		ILanguageModelUtilities2 LanguageModelUtilities { get; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600004C RID: 76
		ICrossReferenceService4 CrossReferenceService { get; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600004D RID: 77
		ILMCompileOptions3 CompileOptions { get; }

		// Token: 0x0600004E RID: 78
		ITargetSettings GetTargetSettingsById(IDeviceIdentification devId);

		// Token: 0x0600004F RID: 79
		ITargetSettings GetSimulationTargetSettings(IDeviceIdentification devId, Guid deviceGuid);

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000050 RID: 80
		IMessageStorage MessageStorage { get; }

		// Token: 0x06000051 RID: 81
		bool IsWarningMessageDisabled(MessageId id);

		// Token: 0x06000052 RID: 82
		bool IsWarningAsError(MessageId id);

		// Token: 0x06000053 RID: 83
		IProgressCallback StartLengthyOperation();

		// Token: 0x06000054 RID: 84
		bool ExistProject(int iProjectHandle);

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000055 RID: 85
		bool ExistsPrimaryProject { get; }

		// Token: 0x06000056 RID: 86
		bool IsPrimaryProject(int iProjectHandle);

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000057 RID: 87
		int PrimaryProjectHandle { get; }

		// Token: 0x06000058 RID: 88
		bool IsLibrary(int iProjectHandle);

		// Token: 0x06000059 RID: 89
		bool IsPrimaryProjectAnInterfaceLibrary();

		// Token: 0x0600005A RID: 90
		bool IsPrimaryProjectALibrary();

		// Token: 0x0600005B RID: 91
		bool IsLibraryWithErrorSummarization(string stLibraryId);

		// Token: 0x0600005C RID: 92
		bool ExistsObject(int nProjectHandle, Guid objectGuid);

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600005D RID: 93
		Guid ActiveApplicationGuid { get; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x0600005E RID: 94
		Guid ProjectInfoObjectGuid { get; }

		// Token: 0x0600005F RID: 95
		Guid GetPoolLibMan(int nProjectHandle);

		// Token: 0x06000060 RID: 96
		ICodegenerator CreateCodegenerator(Guid typeGuid);

		// Token: 0x06000061 RID: 97
		IEnumerable<Guid> GetOpenEditorSignatures();

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000062 RID: 98
		IOEMCustomization OEMCustomization { get; }

		// Token: 0x06000063 RID: 99
		_IPreCompileContext GetLibraryContext(int nProjectHandle);

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000064 RID: 100
		_ICompilerVersionSettings CompilerVersionSettings { get; }

		// Token: 0x06000065 RID: 101
		IVarConfigCodeGenerator TryCreateVarConfigCodeGenerator(Guid typeGuid);

		// Token: 0x06000066 RID: 102
		IAddressCalculator TryCreateAddressCalculator(Guid typeGuid);

		// Token: 0x06000067 RID: 103
		IMemoryAllocationCallback TryCreateMemoryAllocationCallback(Guid typeGuid);

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000068 RID: 104
		IDisplayNameParser DisplayNameParser { get; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000069 RID: 105
		IContainerLibraryChecker ContainerLibraryCheckerOrNull { get; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600006A RID: 106
		INamespaceConflictChecker NamespaceConflictCheckerOrNull { get; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600006B RID: 107
		ICheckAllPoolObjectsConfigurationProvider CheckAllPoolObjectsConfigurationProviderOrNull { get; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600006C RID: 108
		IMemoryStatisticsOutputProvider MemoryStatisticsOutputProviderOrNull { get; }

		// Token: 0x0600006D RID: 109
		ILanguageModelProviderWithDependencies GetLanguageModelProviderWithDependencies(ISourcePosition sourcepos);

		// Token: 0x0600006E RID: 110
		IEnumerable<IExplicitExpressionAtSourcePositionProvider> GetAllExplicitExpressionAtSourcePositionProviders();

		// Token: 0x0600006F RID: 111
		bool GetOnlineApplicationPersistentInfo(Guid gdApplication, out uint uiCRC, out uint uiLength);

		// Token: 0x06000070 RID: 112
		PromptResult Prompt(string stMessage, PromptChoice choice, PromptResult defaultResult, string stMessageKey, params object[] messageArguments);

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000071 RID: 113
		string LibraryDevelopmentOptionsCompilerDefinesToUse { get; }

		// Token: 0x06000072 RID: 114
		IArchiveReader CreateNewBinaryArchiveReader();

		// Token: 0x06000073 RID: 115
		IArchiveReader CreateEncryptedBinaryArchiveReader();

		// Token: 0x06000074 RID: 116
		IArchiveReader CreateNewEncryptedBinaryArchiveReader();

		// Token: 0x06000075 RID: 117
		IArchiveWriter CreateNewLowMemoryFootprintBinaryArchiveWriter();

		// Token: 0x06000076 RID: 118
		ISharedDataStorage GetSharedDataStorage(int nProjectHandle);

		// Token: 0x06000077 RID: 119
		Stream GetParseTreeStreamOfCompiledLibraryPOU(int nProjectHandle, Guid gdObject);

		// Token: 0x06000078 RID: 120
		IPositionTextProvider GetPositionTextProvider(int nProjectHandle, Guid objectGuid);

		// Token: 0x06000079 RID: 121
		IOptionKey CreateSubKey(OptionRoot root, string stSubKey);

		// Token: 0x0600007A RID: 122
		string GetObjectName(int nProjectHandle, Guid objectGuid);

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600007B RID: 123
		_ICheckerThread2 PrecompileChecker { get; }

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600007C RID: 124
		ILicensedSoftwareMetricInformationProvider LicensedSoftwareMetricInformationProvider { get; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600007D RID: 125
		IProjectSideCarService ProjectSideCarService { get; }

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600007E RID: 126
		bool DebugFlagSet { get; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600007F RID: 127
		IParserService ParserService { get; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000080 RID: 128
		IScannerService ScannerService { get; }

		// Token: 0x06000081 RID: 129
		IScannerService GetScannerService(Version version);

		// Token: 0x06000082 RID: 130
		IParserService GetParserService(Version version);

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000083 RID: 131
		ITaskStackSizeProvider TaskStackSizeProvider { get; }

		// Token: 0x06000084 RID: 132
		IEmbeddedLanguageService[] GetEmbeddedLanguageServices();
	}
}
