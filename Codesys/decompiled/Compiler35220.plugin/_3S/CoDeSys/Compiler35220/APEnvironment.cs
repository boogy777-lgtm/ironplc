using System;
using System.Collections.Generic;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelUtilities;
using _3S.CoDeSys.Simulation;

namespace _3S.CoDeSys.Compiler35220
{
	// Token: 0x02000010 RID: 16
	[Obsolete("Use APEnviromentFacade.Instance instead")]
	internal static class APEnvironment
	{
		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000366 RID: 870 RVA: 0x00006C54 File Offset: 0x00004E54
		public static bool InjectionCompleted
		{
			get
			{
				return APEnvironment.\u0001.Value.InjectionCompleted;
			}
		}

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06000367 RID: 871 RVA: 0x00006C68 File Offset: 0x00004E68
		public static _ICompilerVersionSettings CompilerVersionSettings
		{
			get
			{
				return APEnvironment.\u0001.Value.CompilerVersionSettingsProvider.Value;
			}
		}

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06000368 RID: 872 RVA: 0x00006C80 File Offset: 0x00004E80
		public static ICompilerVersionManager7 CompilerVersionEventMgr
		{
			get
			{
				return APEnvironment.\u0001.Value.CompilerVersionEventMgrProvider.Value;
			}
		}

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06000369 RID: 873 RVA: 0x00006C98 File Offset: 0x00004E98
		public static ILanguageModelUtilities2 LanguageModelUtilities
		{
			get
			{
				return APEnvironment.\u0001.Value.LanguageModelUtilitiesProvider.Value;
			}
		}

		// Token: 0x0600036A RID: 874 RVA: 0x00006CB0 File Offset: 0x00004EB0
		public static IVarConfigCodeGenerator \u0001(Guid \u0002)
		{
			return APEnvironment.\u0001.Value.AnyVarConfigCodeGeneratorProvider.TryCreate(\u0002);
		}

		// Token: 0x0600036B RID: 875 RVA: 0x00006CC8 File Offset: 0x00004EC8
		public static IMemoryAllocationCallback \u0001(Guid \u0002)
		{
			return APEnvironment.\u0001.Value.AnyMemoryAllocationCallbackProvider.Create(\u0002);
		}

		// Token: 0x0600036C RID: 876 RVA: 0x00006CE0 File Offset: 0x00004EE0
		public static IAddressCalculator \u0001(Guid \u0002)
		{
			return APEnvironment.\u0001.Value.AnyAddressCalculatorProvider.TryCreate(\u0002);
		}

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x0600036D RID: 877 RVA: 0x00006CF8 File Offset: 0x00004EF8
		public static IOptionStorage OptionStorage
		{
			get
			{
				return APEnvironment.\u0001.Value.OptionStorageProvider.Value;
			}
		}

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x0600036E RID: 878 RVA: 0x00006D10 File Offset: 0x00004F10
		public static ILMCompileOptions3 CompileOptions
		{
			get
			{
				return APEnvironment.\u0001.Value.CompileOptionsProvider.Value;
			}
		}

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x0600036F RID: 879 RVA: 0x00006D28 File Offset: 0x00004F28
		public static _ILibraryDevelopmentOptions LibraryDevelopmentOptions
		{
			get
			{
				return APEnvironment.\u0001.Value.LibraryDevelopmentOptionsProvider.Value;
			}
		}

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06000370 RID: 880 RVA: 0x00006D40 File Offset: 0x00004F40
		public static IEngine9 Engine
		{
			get
			{
				return APEnvironment.\u0001.Value.EngineProvider.Value;
			}
		}

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06000371 RID: 881 RVA: 0x00006D58 File Offset: 0x00004F58
		public static IProjectSideCarService ProjectSideCarService
		{
			get
			{
				return APEnvironment.\u0001.Value.ProjectSideCarServiceProvider.Value;
			}
		}

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06000372 RID: 882 RVA: 0x00006D70 File Offset: 0x00004F70
		public static _ILanguageModelManagerConsolidated LanguageModelMgr
		{
			get
			{
				return APEnvironment.\u0001.Value.LanguageModelMgrProvider.Value;
			}
		}

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06000373 RID: 883 RVA: 0x00006D88 File Offset: 0x00004F88
		public static ILMServiceProvider3 LMServiceProvider
		{
			get
			{
				return APEnvironment.\u0001.Value.LMServiceProviderProvider.Value;
			}
		}

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06000374 RID: 884 RVA: 0x00006DA0 File Offset: 0x00004FA0
		public static IParseTreeStreamProvider ParseTreeStreamProvider
		{
			get
			{
				return APEnvironment.\u0001.Value.ParseTreeStreamProviderProvider.Value;
			}
		}

		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06000375 RID: 885 RVA: 0x00006DB8 File Offset: 0x00004FB8
		public static IDisplayNameParser DisplayNameParser
		{
			get
			{
				return APEnvironment.\u0001.Value.DisplayNameParserProvider.Value;
			}
		}

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06000376 RID: 886 RVA: 0x00006DD0 File Offset: 0x00004FD0
		public static IContainerLibraryChecker ContainerLibraryCheckerOrNull
		{
			get
			{
				return APEnvironment.\u0001.Value.ContainerLibraryCheckerProvider.Value;
			}
		}

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06000377 RID: 887 RVA: 0x00006DE8 File Offset: 0x00004FE8
		public static INamespaceConflictChecker NamespaceConflictCheckerOrNull
		{
			get
			{
				return APEnvironment.\u0001.Value.NamespaceConflictCheckerProvider.Value;
			}
		}

		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06000378 RID: 888 RVA: 0x00006E00 File Offset: 0x00005000
		public static IMessageStorage MessageStorage
		{
			get
			{
				return APEnvironment.\u0001.Value.MessageStorageProvider.Value;
			}
		}

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x06000379 RID: 889 RVA: 0x00006E18 File Offset: 0x00005018
		public static IMessageServiceWithKeys MessageService
		{
			get
			{
				return APEnvironment.\u0001.Value.MessageServiceProvider.Value;
			}
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x0600037A RID: 890 RVA: 0x00006E30 File Offset: 0x00005030
		public static IObjectManager19 ObjectMgr
		{
			get
			{
				return APEnvironment.\u0001.Value.ObjectMgrProvider.Value;
			}
		}

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x0600037B RID: 891 RVA: 0x00006E48 File Offset: 0x00005048
		public static _IWarningHelper WarningHelper
		{
			get
			{
				return APEnvironment.\u0001.Value.WarningHelperProvider.Value;
			}
		}

		// Token: 0x1700031A RID: 794
		// (get) Token: 0x0600037C RID: 892 RVA: 0x00006E60 File Offset: 0x00005060
		public static ITargetSettingsManager TargetSettingsMgr
		{
			get
			{
				return APEnvironment.\u0001.Value.TargetSettingsMgrProvider.Value;
			}
		}

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x0600037D RID: 893 RVA: 0x00006E78 File Offset: 0x00005078
		public static ISimulationManager SimulationManager
		{
			get
			{
				return APEnvironment.\u0001.Value.SimulationManagerProvider.Value;
			}
		}

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x0600037E RID: 894 RVA: 0x00006E90 File Offset: 0x00005090
		public static IOnlineManager OnlineMgr
		{
			get
			{
				return APEnvironment.\u0001.Value.OnlineMgrProvider.Value;
			}
		}

		// Token: 0x0600037F RID: 895 RVA: 0x00006EA8 File Offset: 0x000050A8
		public static ICodegenerator \u0001(Guid \u0002)
		{
			return APEnvironment.\u0001.Value.AnyCodegeneratorProvider.Create(\u0002);
		}

		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06000380 RID: 896 RVA: 0x00006EC0 File Offset: 0x000050C0
		public static ICrossReferenceService4 CrossReferenceService
		{
			get
			{
				return APEnvironment.\u0001.Value.CrossReferenceServiceProvider.Value;
			}
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x06000381 RID: 897 RVA: 0x00006ED8 File Offset: 0x000050D8
		public static ICheckAllPoolObjectsConfigurationProvider CheckAllPoolObjectsConfigurationProviderOrNull
		{
			get
			{
				return APEnvironment.\u0001.Value.CheckAllPoolObjectsConfigurationProviderProvider.Value;
			}
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x06000382 RID: 898 RVA: 0x00006EF0 File Offset: 0x000050F0
		public static IMemoryStatisticsOutputProvider MemoryStatisticsOutputProviderOrNull
		{
			get
			{
				return APEnvironment.\u0001.Value.MemoryStatisticsOutputProviderProvider.Value;
			}
		}

		// Token: 0x06000383 RID: 899 RVA: 0x00006F08 File Offset: 0x00005108
		public static IArchiveWriter \u0001()
		{
			return APEnvironment.\u0001.Value.NewLowMemoryFootprintBinaryArchiveWriterProvider.Create();
		}

		// Token: 0x06000384 RID: 900 RVA: 0x00006F20 File Offset: 0x00005120
		public static IArchiveReader \u0001()
		{
			return APEnvironment.\u0001.Value.NewBinaryArchiveReaderProvider.Create();
		}

		// Token: 0x06000385 RID: 901 RVA: 0x00006F38 File Offset: 0x00005138
		public static IArchiveReader \u0002()
		{
			return APEnvironment.\u0001.Value.NewEncryptedBinaryArchiveReaderProvider.Create();
		}

		// Token: 0x17000320 RID: 800
		// (get) Token: 0x06000386 RID: 902 RVA: 0x00006F50 File Offset: 0x00005150
		public static IEnumerable<IExplicitExpressionAtSourcePositionProvider> ExplicitExpressionAtSourcePositionProviders
		{
			get
			{
				return APEnvironment.\u0001.Value.ExplicitExpressionAtSourcePositionProvidersProvider.Value;
			}
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06000387 RID: 903 RVA: 0x00006F68 File Offset: 0x00005168
		public static ILicensedSoftwareMetricInformationProvider LicensedSoftwareMetricInformationProvider
		{
			get
			{
				return APEnvironment.\u0001.Value.LicensedSoftwareMetricInformationProviderProvider.Value;
			}
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06000388 RID: 904 RVA: 0x00006F80 File Offset: 0x00005180
		public static _IScannerParserProvider2 ScannerParserProvider
		{
			get
			{
				return APEnvironment.\u0001.Value.ScannerParserProviderProvider.Value;
			}
		}

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x06000389 RID: 905 RVA: 0x00006F98 File Offset: 0x00005198
		public static ITaskStackSizeProvider TaskStackSizeProvider
		{
			get
			{
				return APEnvironment.\u0001.Value.TaskStackSizeProviderProvider.Value;
			}
		}

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x0600038A RID: 906 RVA: 0x00006FB0 File Offset: 0x000051B0
		public static IEnumerable<IEmbeddedLanguageService> EmbeddedLanguageServices
		{
			get
			{
				return APEnvironment.\u0001.Value.EmbeddedLanguageServicesProvider.Value;
			}
		}

		// Token: 0x0400000F RID: 15
		private static Lazy<DependencyBag> \u0001 = new Lazy<DependencyBag>(new Func<DependencyBag>(APEnvironment.<>c.<>9.\u0001));
	}
}
