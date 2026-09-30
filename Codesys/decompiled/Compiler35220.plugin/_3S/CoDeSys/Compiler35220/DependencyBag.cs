using System;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.ComponentModel;
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
	// Token: 0x02000012 RID: 18
	internal sealed class DependencyBag : IDependencyInjectable
	{
		// Token: 0x0600038F RID: 911 RVA: 0x00007000 File Offset: 0x00005200
		public DependencyBag()
		{
			ComponentModel.Singleton.InjectDependencies(this, base.GetType());
		}

		// Token: 0x06000390 RID: 912 RVA: 0x0000701C File Offset: 0x0000521C
		public void InjectionComplete()
		{
			this.InjectionCompleted = true;
		}

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06000391 RID: 913 RVA: 0x00007028 File Offset: 0x00005228
		// (set) Token: 0x06000392 RID: 914 RVA: 0x00007030 File Offset: 0x00005230
		public bool InjectionCompleted { get; set; }

		// Token: 0x17000326 RID: 806
		// (get) Token: 0x06000393 RID: 915 RVA: 0x0000703C File Offset: 0x0000523C
		// (set) Token: 0x06000394 RID: 916 RVA: 0x00007044 File Offset: 0x00005244
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IEngine9> EngineProvider { get; private set; }

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x06000395 RID: 917 RVA: 0x00007050 File Offset: 0x00005250
		// (set) Token: 0x06000396 RID: 918 RVA: 0x00007058 File Offset: 0x00005258
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IProjectSideCarService> ProjectSideCarServiceProvider { get; private set; }

		// Token: 0x17000328 RID: 808
		// (get) Token: 0x06000397 RID: 919 RVA: 0x00007064 File Offset: 0x00005264
		// (set) Token: 0x06000398 RID: 920 RVA: 0x0000706C File Offset: 0x0000526C
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IMessageServiceWithKeys> MessageServiceProvider { get; private set; }

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x06000399 RID: 921 RVA: 0x00007078 File Offset: 0x00005278
		// (set) Token: 0x0600039A RID: 922 RVA: 0x00007080 File Offset: 0x00005280
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_ILanguageModelManagerConsolidated> LanguageModelMgrProvider { get; private set; }

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x0600039B RID: 923 RVA: 0x0000708C File Offset: 0x0000528C
		// (set) Token: 0x0600039C RID: 924 RVA: 0x00007094 File Offset: 0x00005294
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILMServiceProvider3> LMServiceProviderProvider { get; private set; }

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x0600039D RID: 925 RVA: 0x000070A0 File Offset: 0x000052A0
		// (set) Token: 0x0600039E RID: 926 RVA: 0x000070A8 File Offset: 0x000052A8
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IParseTreeStreamProvider> ParseTreeStreamProviderProvider { get; private set; }

		// Token: 0x1700032C RID: 812
		// (get) Token: 0x0600039F RID: 927 RVA: 0x000070B4 File Offset: 0x000052B4
		// (set) Token: 0x060003A0 RID: 928 RVA: 0x000070BC File Offset: 0x000052BC
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IMessageStorage> MessageStorageProvider { get; private set; }

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x060003A1 RID: 929 RVA: 0x000070C8 File Offset: 0x000052C8
		// (set) Token: 0x060003A2 RID: 930 RVA: 0x000070D0 File Offset: 0x000052D0
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_IWarningHelper> WarningHelperProvider { get; private set; }

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x060003A3 RID: 931 RVA: 0x000070DC File Offset: 0x000052DC
		// (set) Token: 0x060003A4 RID: 932 RVA: 0x000070E4 File Offset: 0x000052E4
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IObjectManager19> ObjectMgrProvider { get; private set; }

		// Token: 0x1700032F RID: 815
		// (get) Token: 0x060003A5 RID: 933 RVA: 0x000070F0 File Offset: 0x000052F0
		// (set) Token: 0x060003A6 RID: 934 RVA: 0x000070F8 File Offset: 0x000052F8
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IOnlineManager> OnlineMgrProvider { get; private set; }

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x060003A7 RID: 935 RVA: 0x00007104 File Offset: 0x00005304
		// (set) Token: 0x060003A8 RID: 936 RVA: 0x0000710C File Offset: 0x0000530C
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ITargetSettingsManager> TargetSettingsMgrProvider { get; private set; }

		// Token: 0x17000331 RID: 817
		// (get) Token: 0x060003A9 RID: 937 RVA: 0x00007118 File Offset: 0x00005318
		// (set) Token: 0x060003AA RID: 938 RVA: 0x00007120 File Offset: 0x00005320
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILMCompileOptions3> CompileOptionsProvider { get; private set; }

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x060003AB RID: 939 RVA: 0x0000712C File Offset: 0x0000532C
		// (set) Token: 0x060003AC RID: 940 RVA: 0x00007134 File Offset: 0x00005334
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_ILibraryDevelopmentOptions> LibraryDevelopmentOptionsProvider { get; private set; }

		// Token: 0x17000333 RID: 819
		// (get) Token: 0x060003AD RID: 941 RVA: 0x00007140 File Offset: 0x00005340
		// (set) Token: 0x060003AE RID: 942 RVA: 0x00007148 File Offset: 0x00005348
		[InjectAnyInstance]
		public IAnyInstanceProvider<ICodegenerator> AnyCodegeneratorProvider { get; private set; }

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x060003AF RID: 943 RVA: 0x00007154 File Offset: 0x00005354
		// (set) Token: 0x060003B0 RID: 944 RVA: 0x0000715C File Offset: 0x0000535C
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILanguageModelUtilities2> LanguageModelUtilitiesProvider { get; private set; }

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x060003B1 RID: 945 RVA: 0x00007168 File Offset: 0x00005368
		// (set) Token: 0x060003B2 RID: 946 RVA: 0x00007170 File Offset: 0x00005370
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ISimulationManager> SimulationManagerProvider { get; private set; }

		// Token: 0x17000336 RID: 822
		// (get) Token: 0x060003B3 RID: 947 RVA: 0x0000717C File Offset: 0x0000537C
		// (set) Token: 0x060003B4 RID: 948 RVA: 0x00007184 File Offset: 0x00005384
		[InjectAnyInstance]
		public IAnyInstanceProvider<IVarConfigCodeGenerator> AnyVarConfigCodeGeneratorProvider { get; private set; }

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x060003B5 RID: 949 RVA: 0x00007190 File Offset: 0x00005390
		// (set) Token: 0x060003B6 RID: 950 RVA: 0x00007198 File Offset: 0x00005398
		[InjectAnyInstance]
		public IAnyInstanceProvider<IMemoryAllocationCallback> AnyMemoryAllocationCallbackProvider { get; private set; }

		// Token: 0x17000338 RID: 824
		// (get) Token: 0x060003B7 RID: 951 RVA: 0x000071A4 File Offset: 0x000053A4
		// (set) Token: 0x060003B8 RID: 952 RVA: 0x000071AC File Offset: 0x000053AC
		[InjectAnyInstance]
		public IAnyInstanceProvider<IAddressCalculator> AnyAddressCalculatorProvider { get; private set; }

		// Token: 0x17000339 RID: 825
		// (get) Token: 0x060003B9 RID: 953 RVA: 0x000071B8 File Offset: 0x000053B8
		// (set) Token: 0x060003BA RID: 954 RVA: 0x000071C0 File Offset: 0x000053C0
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IDisplayNameParser> DisplayNameParserProvider { get; private set; }

		// Token: 0x1700033A RID: 826
		// (get) Token: 0x060003BB RID: 955 RVA: 0x000071CC File Offset: 0x000053CC
		// (set) Token: 0x060003BC RID: 956 RVA: 0x000071D4 File Offset: 0x000053D4
		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<IContainerLibraryChecker> ContainerLibraryCheckerProvider { get; private set; }

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x060003BD RID: 957 RVA: 0x000071E0 File Offset: 0x000053E0
		// (set) Token: 0x060003BE RID: 958 RVA: 0x000071E8 File Offset: 0x000053E8
		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<INamespaceConflictChecker> NamespaceConflictCheckerProvider { get; private set; }

		// Token: 0x1700033C RID: 828
		// (get) Token: 0x060003BF RID: 959 RVA: 0x000071F4 File Offset: 0x000053F4
		// (set) Token: 0x060003C0 RID: 960 RVA: 0x000071FC File Offset: 0x000053FC
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_ICompilerVersionSettings> CompilerVersionSettingsProvider { get; private set; }

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x060003C1 RID: 961 RVA: 0x00007208 File Offset: 0x00005408
		// (set) Token: 0x060003C2 RID: 962 RVA: 0x00007210 File Offset: 0x00005410
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IOptionStorage> OptionStorageProvider { get; private set; }

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x060003C3 RID: 963 RVA: 0x0000721C File Offset: 0x0000541C
		// (set) Token: 0x060003C4 RID: 964 RVA: 0x00007224 File Offset: 0x00005424
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ICrossReferenceService4> CrossReferenceServiceProvider { get; private set; }

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x060003C5 RID: 965 RVA: 0x00007230 File Offset: 0x00005430
		// (set) Token: 0x060003C6 RID: 966 RVA: 0x00007238 File Offset: 0x00005438
		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<ICheckAllPoolObjectsConfigurationProvider> CheckAllPoolObjectsConfigurationProviderProvider { get; private set; }

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x060003C7 RID: 967 RVA: 0x00007244 File Offset: 0x00005444
		// (set) Token: 0x060003C8 RID: 968 RVA: 0x0000724C File Offset: 0x0000544C
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ICompilerVersionManager7> CompilerVersionEventMgrProvider { get; private set; }

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x060003C9 RID: 969 RVA: 0x00007258 File Offset: 0x00005458
		// (set) Token: 0x060003CA RID: 970 RVA: 0x00007260 File Offset: 0x00005460
		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<IMemoryStatisticsOutputProvider> MemoryStatisticsOutputProviderProvider { get; private set; }

		// Token: 0x17000342 RID: 834
		// (get) Token: 0x060003CB RID: 971 RVA: 0x0000726C File Offset: 0x0000546C
		// (set) Token: 0x060003CC RID: 972 RVA: 0x00007274 File Offset: 0x00005474
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveWriter> NewLowMemoryFootprintBinaryArchiveWriterProvider { get; private set; }

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x060003CD RID: 973 RVA: 0x00007280 File Offset: 0x00005480
		// (set) Token: 0x060003CE RID: 974 RVA: 0x00007288 File Offset: 0x00005488
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveReader> NewBinaryArchiveReaderProvider { get; private set; }

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x060003CF RID: 975 RVA: 0x00007294 File Offset: 0x00005494
		// (set) Token: 0x060003D0 RID: 976 RVA: 0x0000729C File Offset: 0x0000549C
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveReader> NewEncryptedBinaryArchiveReaderProvider { get; private set; }

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x060003D1 RID: 977 RVA: 0x000072A8 File Offset: 0x000054A8
		// (set) Token: 0x060003D2 RID: 978 RVA: 0x000072B0 File Offset: 0x000054B0
		[InjectMultipleInstances(Shared = true, Optional = true)]
		public ISharedMultipleInstancesProvider<IExplicitExpressionAtSourcePositionProvider> ExplicitExpressionAtSourcePositionProvidersProvider { get; private set; }

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x060003D3 RID: 979 RVA: 0x000072BC File Offset: 0x000054BC
		// (set) Token: 0x060003D4 RID: 980 RVA: 0x000072C4 File Offset: 0x000054C4
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILicensedSoftwareMetricInformationProvider> LicensedSoftwareMetricInformationProviderProvider { get; private set; }

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x060003D5 RID: 981 RVA: 0x000072D0 File Offset: 0x000054D0
		// (set) Token: 0x060003D6 RID: 982 RVA: 0x000072D8 File Offset: 0x000054D8
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_IScannerParserProvider2> ScannerParserProviderProvider { get; private set; }

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x060003D7 RID: 983 RVA: 0x000072E4 File Offset: 0x000054E4
		// (set) Token: 0x060003D8 RID: 984 RVA: 0x000072EC File Offset: 0x000054EC
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ITaskStackSizeProvider> TaskStackSizeProviderProvider { get; private set; }

		// Token: 0x17000349 RID: 841
		// (get) Token: 0x060003D9 RID: 985 RVA: 0x000072F8 File Offset: 0x000054F8
		// (set) Token: 0x060003DA RID: 986 RVA: 0x00007300 File Offset: 0x00005500
		[InjectMultipleInstances(Shared = true, Optional = true)]
		public ISharedMultipleInstancesProvider<IEmbeddedLanguageService> EmbeddedLanguageServicesProvider { get; private set; }
	}
}
