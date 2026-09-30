using System;
using System.Collections.Generic;
using CODESYS.Parser;
using CODESYS.ProjectFormat.SideCar;
using CODESYS.ProjectLanguageModelProvider;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Commands;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.Interfaces;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.OnlineExpressionInterpreter;
using _3S.CoDeSys.Simulation;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000BB RID: 187
	[Obsolete("Use APEnviromentFacade.Instance instead")]
	internal static class APEnvironment
	{
		// Token: 0x17000283 RID: 643
		// (get) Token: 0x06000AF5 RID: 2805 RVA: 0x0001C7BA File Offset: 0x0001B7BA
		public static bool InjectionCompleted
		{
			get
			{
				return APEnvironment.s_bag.Value.InjectionCompleted;
			}
		}

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x06000AF6 RID: 2806 RVA: 0x0001C7CB File Offset: 0x0001B7CB
		public static CompilerVersionManager CompilerVersionMgr
		{
			get
			{
				return APEnvironment.s_bag.Value.CompilerVersionMgrProvider.Value;
			}
		}

		// Token: 0x06000AF7 RID: 2807 RVA: 0x0001C7E1 File Offset: 0x0001B7E1
		public static IEnumerable<_ICompilerVersionsProvider> CreateCompilerVersionsProviders()
		{
			return APEnvironment.s_bag.Value.CompilerVersionsProvidersProvider.Create();
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x06000AF8 RID: 2808 RVA: 0x0001C7F7 File Offset: 0x0001B7F7
		public static IOptionStorage OptionStorage
		{
			get
			{
				return APEnvironment.s_bag.Value.OptionStorageProvider.Value;
			}
		}

		// Token: 0x06000AF9 RID: 2809 RVA: 0x0001C80D File Offset: 0x0001B80D
		public static IEnumerable<IAttributeProvider> CreateAttributeProviders()
		{
			return APEnvironment.s_bag.Value.AttributeProvidersProvider.Create();
		}

		// Token: 0x06000AFA RID: 2810 RVA: 0x0001C823 File Offset: 0x0001B823
		public static IOnlineExpressionInterpreter3 CreateOnlineExpressionInterpreter()
		{
			return APEnvironment.s_bag.Value.OnlineExpressionInterpreterProvider.Create();
		}

		// Token: 0x06000AFB RID: 2811 RVA: 0x0001C839 File Offset: 0x0001B839
		public static ILibraryPlaceholderResolutionEx CreateLibraryPlaceholderResolutionEx()
		{
			return APEnvironment.s_bag.Value.LibraryPlaceholderResolutionExProvider.Create();
		}

		// Token: 0x17000286 RID: 646
		// (get) Token: 0x06000AFC RID: 2812 RVA: 0x0001C84F File Offset: 0x0001B84F
		public static Guid LibraryPlaceholderResolutionExGuid
		{
			get
			{
				return APEnvironment.s_bag.Value.LibraryPlaceholderResolutionExProvider.TypeGuid;
			}
		}

		// Token: 0x06000AFD RID: 2813 RVA: 0x0001C865 File Offset: 0x0001B865
		public static IArchiveReader CreateNewBinaryArchiveReader()
		{
			return APEnvironment.s_bag.Value.NewBinaryArchiveReaderProvider.Create();
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x0001C87B File Offset: 0x0001B87B
		public static IArchiveReader CreateBinaryArchiveReader()
		{
			return APEnvironment.s_bag.Value.BinaryArchiveReaderProvider.Create();
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x0001C891 File Offset: 0x0001B891
		public static IArchiveWriter CreateNewLowMemoryFootprintBinaryArchiveWriter()
		{
			return APEnvironment.s_bag.Value.NewLowMemoryFootprintBinaryArchiveWriterProvider.Create();
		}

		// Token: 0x06000B00 RID: 2816 RVA: 0x0001C8A7 File Offset: 0x0001B8A7
		public static IArchiveWriter CreateNewBinaryArchiveWriter()
		{
			return APEnvironment.s_bag.Value.NewBinaryArchiveWriterProvider.Create();
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x0001C8BD File Offset: 0x0001B8BD
		public static IArchiveWriter CreateBinaryArchiveWriter()
		{
			return APEnvironment.s_bag.Value.BinaryArchiveWriterProvider.Create();
		}

		// Token: 0x06000B02 RID: 2818 RVA: 0x0001C8D3 File Offset: 0x0001B8D3
		public static IArchiveReader CreateArchiveReader(Guid typeGuid)
		{
			return APEnvironment.s_bag.Value.AnyArchiveReaderProvider.Create(typeGuid);
		}

		// Token: 0x06000B03 RID: 2819 RVA: 0x0001C8EA File Offset: 0x0001B8EA
		public static IArchiveReader CreateEncryptedBinaryArchiveReader()
		{
			return APEnvironment.s_bag.Value.EncryptedBinaryArchiveReaderProvider.Create();
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x0001C900 File Offset: 0x0001B900
		public static IArchiveReader CreateNewEncryptedBinaryArchiveReader()
		{
			return APEnvironment.s_bag.Value.NewEncryptedBinaryArchiveReaderProvider.Create();
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x0001C916 File Offset: 0x0001B916
		public static object TryCreateResolver(Guid typeGuid)
		{
			return APEnvironment.s_bag.Value.AnyResolverProvider.TryCreate(typeGuid);
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x0001C92D File Offset: 0x0001B92D
		public static IStandardCommand CreateCleanAllCommand()
		{
			return APEnvironment.s_bag.Value.CleanAllCommandProvider.Create();
		}

		// Token: 0x17000287 RID: 647
		// (get) Token: 0x06000B07 RID: 2823 RVA: 0x0001C943 File Offset: 0x0001B943
		public static Guid CleanAllCommandGuid
		{
			get
			{
				return APEnvironment.s_bag.Value.CleanAllCommandProvider.TypeGuid;
			}
		}

		// Token: 0x17000288 RID: 648
		// (get) Token: 0x06000B08 RID: 2824 RVA: 0x0001C959 File Offset: 0x0001B959
		public static Profile Profile
		{
			get
			{
				return APEnvironment.s_bag.Value.ProfileInformation.Profile;
			}
		}

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x06000B09 RID: 2825 RVA: 0x0001C96F File Offset: 0x0001B96F
		public static _ICompileOptions2 CompileOptions
		{
			get
			{
				return APEnvironment.s_bag.Value.CompileOptionsProvider.Value;
			}
		}

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06000B0A RID: 2826 RVA: 0x0001C985 File Offset: 0x0001B985
		public static _ILibraryDevelopmentOptions LibraryDevelopmentOptions
		{
			get
			{
				return APEnvironment.s_bag.Value.LibraryDevelopmentOptionsProvider.Value;
			}
		}

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06000B0B RID: 2827 RVA: 0x0001C99B File Offset: 0x0001B99B
		public static IEngine9 Engine
		{
			get
			{
				return APEnvironment.s_bag.Value.EngineProvider.Value;
			}
		}

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06000B0C RID: 2828 RVA: 0x0001C9B1 File Offset: 0x0001B9B1
		public static IProjectSideCarService ProjectSideCarService
		{
			get
			{
				return APEnvironment.s_bag.Value.ProjectSideCarServiceProvider.Value;
			}
		}

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06000B0D RID: 2829 RVA: 0x0001C9C7 File Offset: 0x0001B9C7
		public static LanguageModelManagerConsolidated LanguageModelMgr
		{
			get
			{
				return APEnvironment.s_bag.Value.LanguageModelMgrProvider.Value;
			}
		}

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x06000B0E RID: 2830 RVA: 0x0001C9DD File Offset: 0x0001B9DD
		public static _ILMServiceProvider3 LMServiceProvider
		{
			get
			{
				return APEnvironment.s_bag.Value.LMServiceProviderProvider.Value;
			}
		}

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x06000B0F RID: 2831 RVA: 0x0001C9F3 File Offset: 0x0001B9F3
		public static ILibraryLoader16 LibraryLoader
		{
			get
			{
				return APEnvironment.s_bag.Value.LibraryLoaderProvider.Value;
			}
		}

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x06000B10 RID: 2832 RVA: 0x0001CA09 File Offset: 0x0001BA09
		public static IParseTreeStreamProvider ParseTreeStreamProvider
		{
			get
			{
				return APEnvironment.s_bag.Value.ParseTreeStreamProviderProvider.Value;
			}
		}

		// Token: 0x17000291 RID: 657
		// (get) Token: 0x06000B11 RID: 2833 RVA: 0x0001CA1F File Offset: 0x0001BA1F
		public static INamespaceConflictChecker NamespaceConflictCheckerOrNull
		{
			get
			{
				return APEnvironment.s_bag.Value.NamespaceConflictCheckerProvider.Value;
			}
		}

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06000B12 RID: 2834 RVA: 0x0001CA35 File Offset: 0x0001BA35
		public static IMessageStorage MessageStorage
		{
			get
			{
				return APEnvironment.s_bag.Value.MessageStorageProvider.Value;
			}
		}

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06000B13 RID: 2835 RVA: 0x0001CA4B File Offset: 0x0001BA4B
		public static IMessageServiceWithKeys MessageService
		{
			get
			{
				return APEnvironment.s_bag.Value.MessageServiceProvider.Value;
			}
		}

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06000B14 RID: 2836 RVA: 0x0001CA61 File Offset: 0x0001BA61
		public static IObjectManager19 ObjectMgr
		{
			get
			{
				return APEnvironment.s_bag.Value.ObjectMgrProvider.Value;
			}
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06000B15 RID: 2837 RVA: 0x0001CA77 File Offset: 0x0001BA77
		public static _IWarningHelper WarningHelper
		{
			get
			{
				return APEnvironment.s_bag.Value.WarningHelperProvider.Value;
			}
		}

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x06000B16 RID: 2838 RVA: 0x0001CA8D File Offset: 0x0001BA8D
		public static ITargetSettingsManager TargetSettingsMgr
		{
			get
			{
				return APEnvironment.s_bag.Value.TargetSettingsMgrProvider.Value;
			}
		}

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x06000B17 RID: 2839 RVA: 0x0001CAA3 File Offset: 0x0001BAA3
		public static ITargetSettingsProvider TargetSettingsProvider
		{
			get
			{
				return APEnvironment.s_bag.Value.TargetSettingsProviderProvider.Value;
			}
		}

		// Token: 0x17000298 RID: 664
		// (get) Token: 0x06000B18 RID: 2840 RVA: 0x0001CAB9 File Offset: 0x0001BAB9
		public static ISimulationManager SimulationManager
		{
			get
			{
				return APEnvironment.s_bag.Value.SimulationManagerProvider.Value;
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06000B19 RID: 2841 RVA: 0x0001CACF File Offset: 0x0001BACF
		public static IOnlineManager OnlineMgr
		{
			get
			{
				return APEnvironment.s_bag.Value.OnlineMgrProvider.Value;
			}
		}

		// Token: 0x06000B1A RID: 2842 RVA: 0x0001CAE5 File Offset: 0x0001BAE5
		public static ICodegenerator CreateCodegenerator(Guid typeGuid)
		{
			return APEnvironment.s_bag.Value.AnyCodegeneratorProvider.Create(typeGuid);
		}

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06000B1B RID: 2843 RVA: 0x0001CAFC File Offset: 0x0001BAFC
		public static IProjectLanguageModelProvider ProjectLanguageModel
		{
			get
			{
				return APEnvironment.s_bag.Value.ProjectLanguageModelProvider.Value;
			}
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x0001CB12 File Offset: 0x0001BB12
		public static IEnumerable<IParserService> CreateParserServiceProviders()
		{
			return APEnvironment.s_bag.Value.ParserServiceProvidersProvider.Create();
		}

		// Token: 0x06000B1D RID: 2845 RVA: 0x0001CB28 File Offset: 0x0001BB28
		public static IEnumerable<IScannerService> CreateScannerServiceProviders()
		{
			return APEnvironment.s_bag.Value.ScannerServiceProvidersProvider.Create();
		}

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06000B1E RID: 2846 RVA: 0x0001CB3E File Offset: 0x0001BB3E
		public static _IScannerParserProvider ScannerParserProvider
		{
			get
			{
				return APEnvironment.s_bag.Value.ScannerParserProviderProvider.Value;
			}
		}

		// Token: 0x040001BE RID: 446
		private static Lazy<DependencyBag> s_bag = new Lazy<DependencyBag>(() => new DependencyBag());
	}
}
