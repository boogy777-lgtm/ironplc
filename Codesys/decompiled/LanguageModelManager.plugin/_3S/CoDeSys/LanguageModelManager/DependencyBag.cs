using System;
using CODESYS.Parser;
using CODESYS.ProjectFormat.SideCar;
using CODESYS.ProjectLanguageModelProvider;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Commands;
using _3S.CoDeSys.Core.ComponentModel;
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
	// Token: 0x020000BC RID: 188
	internal class DependencyBag : IDependencyInjectable
	{
		// Token: 0x06000B20 RID: 2848 RVA: 0x0001CB70 File Offset: 0x0001BB70
		public DependencyBag()
		{
			ComponentModel.Singleton.InjectDependencies(this, base.GetType());
		}

		// Token: 0x06000B21 RID: 2849 RVA: 0x0001CB89 File Offset: 0x0001BB89
		public void InjectionComplete()
		{
			this.InjectionCompleted = true;
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06000B22 RID: 2850 RVA: 0x0001CB92 File Offset: 0x0001BB92
		// (set) Token: 0x06000B23 RID: 2851 RVA: 0x0001CB9A File Offset: 0x0001BB9A
		public bool InjectionCompleted { get; set; }

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06000B24 RID: 2852 RVA: 0x0001CBA3 File Offset: 0x0001BBA3
		// (set) Token: 0x06000B25 RID: 2853 RVA: 0x0001CBAB File Offset: 0x0001BBAB
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IEngine9> EngineProvider { get; private set; }

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x06000B26 RID: 2854 RVA: 0x0001CBB4 File Offset: 0x0001BBB4
		// (set) Token: 0x06000B27 RID: 2855 RVA: 0x0001CBBC File Offset: 0x0001BBBC
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IProjectSideCarService> ProjectSideCarServiceProvider { get; private set; }

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06000B28 RID: 2856 RVA: 0x0001CBC5 File Offset: 0x0001BBC5
		// (set) Token: 0x06000B29 RID: 2857 RVA: 0x0001CBCD File Offset: 0x0001BBCD
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IMessageServiceWithKeys> MessageServiceProvider { get; private set; }

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06000B2A RID: 2858 RVA: 0x0001CBD6 File Offset: 0x0001BBD6
		// (set) Token: 0x06000B2B RID: 2859 RVA: 0x0001CBDE File Offset: 0x0001BBDE
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<LanguageModelManagerConsolidated> LanguageModelMgrProvider { get; private set; }

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06000B2C RID: 2860 RVA: 0x0001CBE7 File Offset: 0x0001BBE7
		// (set) Token: 0x06000B2D RID: 2861 RVA: 0x0001CBEF File Offset: 0x0001BBEF
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_ILMServiceProvider3> LMServiceProviderProvider { get; private set; }

		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06000B2E RID: 2862 RVA: 0x0001CBF8 File Offset: 0x0001BBF8
		// (set) Token: 0x06000B2F RID: 2863 RVA: 0x0001CC00 File Offset: 0x0001BC00
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IMessageStorage> MessageStorageProvider { get; private set; }

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06000B30 RID: 2864 RVA: 0x0001CC09 File Offset: 0x0001BC09
		// (set) Token: 0x06000B31 RID: 2865 RVA: 0x0001CC11 File Offset: 0x0001BC11
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_IWarningHelper> WarningHelperProvider { get; private set; }

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x06000B32 RID: 2866 RVA: 0x0001CC1A File Offset: 0x0001BC1A
		// (set) Token: 0x06000B33 RID: 2867 RVA: 0x0001CC22 File Offset: 0x0001BC22
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IObjectManager19> ObjectMgrProvider { get; private set; }

		// Token: 0x170002A5 RID: 677
		// (get) Token: 0x06000B34 RID: 2868 RVA: 0x0001CC2B File Offset: 0x0001BC2B
		// (set) Token: 0x06000B35 RID: 2869 RVA: 0x0001CC33 File Offset: 0x0001BC33
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IOnlineManager> OnlineMgrProvider { get; private set; }

		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06000B36 RID: 2870 RVA: 0x0001CC3C File Offset: 0x0001BC3C
		// (set) Token: 0x06000B37 RID: 2871 RVA: 0x0001CC44 File Offset: 0x0001BC44
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ITargetSettingsManager> TargetSettingsMgrProvider { get; private set; }

		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x06000B38 RID: 2872 RVA: 0x0001CC4D File Offset: 0x0001BC4D
		// (set) Token: 0x06000B39 RID: 2873 RVA: 0x0001CC55 File Offset: 0x0001BC55
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_ICompileOptions2> CompileOptionsProvider { get; private set; }

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x06000B3A RID: 2874 RVA: 0x0001CC5E File Offset: 0x0001BC5E
		// (set) Token: 0x06000B3B RID: 2875 RVA: 0x0001CC66 File Offset: 0x0001BC66
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_ILibraryDevelopmentOptions> LibraryDevelopmentOptionsProvider { get; private set; }

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x06000B3C RID: 2876 RVA: 0x0001CC6F File Offset: 0x0001BC6F
		// (set) Token: 0x06000B3D RID: 2877 RVA: 0x0001CC77 File Offset: 0x0001BC77
		[InjectAnyInstance]
		public IAnyInstanceProvider<ICodegenerator> AnyCodegeneratorProvider { get; private set; }

		// Token: 0x170002AA RID: 682
		// (get) Token: 0x06000B3E RID: 2878 RVA: 0x0001CC80 File Offset: 0x0001BC80
		// (set) Token: 0x06000B3F RID: 2879 RVA: 0x0001CC88 File Offset: 0x0001BC88
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ITargetSettingsProvider> TargetSettingsProviderProvider { get; private set; }

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x06000B40 RID: 2880 RVA: 0x0001CC91 File Offset: 0x0001BC91
		// (set) Token: 0x06000B41 RID: 2881 RVA: 0x0001CC99 File Offset: 0x0001BC99
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ISimulationManager> SimulationManagerProvider { get; private set; }

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x06000B42 RID: 2882 RVA: 0x0001CCA2 File Offset: 0x0001BCA2
		// (set) Token: 0x06000B43 RID: 2883 RVA: 0x0001CCAA File Offset: 0x0001BCAA
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<ILibraryLoader16> LibraryLoaderProvider { get; private set; }

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x06000B44 RID: 2884 RVA: 0x0001CCB3 File Offset: 0x0001BCB3
		// (set) Token: 0x06000B45 RID: 2885 RVA: 0x0001CCBB File Offset: 0x0001BCBB
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IParseTreeStreamProvider> ParseTreeStreamProviderProvider { get; private set; }

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x06000B46 RID: 2886 RVA: 0x0001CCC4 File Offset: 0x0001BCC4
		// (set) Token: 0x06000B47 RID: 2887 RVA: 0x0001CCCC File Offset: 0x0001BCCC
		[InjectSingleInstance(Shared = true, Optional = true)]
		public ISharedSingleInstanceProvider<INamespaceConflictChecker> NamespaceConflictCheckerProvider { get; private set; }

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x06000B48 RID: 2888 RVA: 0x0001CCD5 File Offset: 0x0001BCD5
		// (set) Token: 0x06000B49 RID: 2889 RVA: 0x0001CCDD File Offset: 0x0001BCDD
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<CompilerVersionManager> CompilerVersionMgrProvider { get; private set; }

		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x06000B4A RID: 2890 RVA: 0x0001CCE6 File Offset: 0x0001BCE6
		// (set) Token: 0x06000B4B RID: 2891 RVA: 0x0001CCEE File Offset: 0x0001BCEE
		[InjectMultipleInstances(Optional = true)]
		public IMultipleInstancesProvider<_ICompilerVersionsProvider> CompilerVersionsProvidersProvider { get; private set; }

		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x06000B4C RID: 2892 RVA: 0x0001CCF7 File Offset: 0x0001BCF7
		// (set) Token: 0x06000B4D RID: 2893 RVA: 0x0001CCFF File Offset: 0x0001BCFF
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IOptionStorage> OptionStorageProvider { get; private set; }

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x06000B4E RID: 2894 RVA: 0x0001CD08 File Offset: 0x0001BD08
		// (set) Token: 0x06000B4F RID: 2895 RVA: 0x0001CD10 File Offset: 0x0001BD10
		[InjectMultipleInstances(Optional = true)]
		public IMultipleInstancesProvider<IAttributeProvider> AttributeProvidersProvider { get; private set; }

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x06000B50 RID: 2896 RVA: 0x0001CD19 File Offset: 0x0001BD19
		// (set) Token: 0x06000B51 RID: 2897 RVA: 0x0001CD21 File Offset: 0x0001BD21
		[InjectSingleInstance]
		public ISingleInstanceProvider<IOnlineExpressionInterpreter3> OnlineExpressionInterpreterProvider { get; private set; }

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x06000B52 RID: 2898 RVA: 0x0001CD2A File Offset: 0x0001BD2A
		// (set) Token: 0x06000B53 RID: 2899 RVA: 0x0001CD32 File Offset: 0x0001BD32
		[InjectSingleInstance]
		public ISingleInstanceProvider<ILibraryPlaceholderResolutionEx> LibraryPlaceholderResolutionExProvider { get; private set; }

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x06000B54 RID: 2900 RVA: 0x0001CD3B File Offset: 0x0001BD3B
		// (set) Token: 0x06000B55 RID: 2901 RVA: 0x0001CD43 File Offset: 0x0001BD43
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveWriter> NewLowMemoryFootprintBinaryArchiveWriterProvider { get; private set; }

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x06000B56 RID: 2902 RVA: 0x0001CD4C File Offset: 0x0001BD4C
		// (set) Token: 0x06000B57 RID: 2903 RVA: 0x0001CD54 File Offset: 0x0001BD54
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveWriter> NewBinaryArchiveWriterProvider { get; private set; }

		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06000B58 RID: 2904 RVA: 0x0001CD5D File Offset: 0x0001BD5D
		// (set) Token: 0x06000B59 RID: 2905 RVA: 0x0001CD65 File Offset: 0x0001BD65
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveReader> NewBinaryArchiveReaderProvider { get; private set; }

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06000B5A RID: 2906 RVA: 0x0001CD6E File Offset: 0x0001BD6E
		// (set) Token: 0x06000B5B RID: 2907 RVA: 0x0001CD76 File Offset: 0x0001BD76
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveWriter> BinaryArchiveWriterProvider { get; private set; }

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x06000B5C RID: 2908 RVA: 0x0001CD7F File Offset: 0x0001BD7F
		// (set) Token: 0x06000B5D RID: 2909 RVA: 0x0001CD87 File Offset: 0x0001BD87
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveReader> BinaryArchiveReaderProvider { get; private set; }

		// Token: 0x170002BA RID: 698
		// (get) Token: 0x06000B5E RID: 2910 RVA: 0x0001CD90 File Offset: 0x0001BD90
		// (set) Token: 0x06000B5F RID: 2911 RVA: 0x0001CD98 File Offset: 0x0001BD98
		[InjectAnyInstance]
		public IAnyInstanceProvider<IArchiveReader> AnyArchiveReaderProvider { get; private set; }

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x06000B60 RID: 2912 RVA: 0x0001CDA1 File Offset: 0x0001BDA1
		// (set) Token: 0x06000B61 RID: 2913 RVA: 0x0001CDA9 File Offset: 0x0001BDA9
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveReader> EncryptedBinaryArchiveReaderProvider { get; private set; }

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x06000B62 RID: 2914 RVA: 0x0001CDB2 File Offset: 0x0001BDB2
		// (set) Token: 0x06000B63 RID: 2915 RVA: 0x0001CDBA File Offset: 0x0001BDBA
		[InjectSingleInstance]
		public ISingleInstanceProvider<IArchiveReader> NewEncryptedBinaryArchiveReaderProvider { get; private set; }

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06000B64 RID: 2916 RVA: 0x0001CDC3 File Offset: 0x0001BDC3
		// (set) Token: 0x06000B65 RID: 2917 RVA: 0x0001CDCB File Offset: 0x0001BDCB
		[InjectAnyInstance]
		public IAnyInstanceProvider<object> AnyResolverProvider { get; private set; }

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x06000B66 RID: 2918 RVA: 0x0001CDD4 File Offset: 0x0001BDD4
		// (set) Token: 0x06000B67 RID: 2919 RVA: 0x0001CDDC File Offset: 0x0001BDDC
		[InjectSingleInstance]
		public ISingleInstanceProvider<IStandardCommand> CleanAllCommandProvider { get; private set; }

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x06000B68 RID: 2920 RVA: 0x0001CDE5 File Offset: 0x0001BDE5
		// (set) Token: 0x06000B69 RID: 2921 RVA: 0x0001CDED File Offset: 0x0001BDED
		[InjectActiveProfile]
		public IProfileInformation ProfileInformation { get; private set; }

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x06000B6A RID: 2922 RVA: 0x0001CDF6 File Offset: 0x0001BDF6
		// (set) Token: 0x06000B6B RID: 2923 RVA: 0x0001CDFE File Offset: 0x0001BDFE
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<IProjectLanguageModelProvider> ProjectLanguageModelProvider { get; private set; }

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x06000B6C RID: 2924 RVA: 0x0001CE07 File Offset: 0x0001BE07
		// (set) Token: 0x06000B6D RID: 2925 RVA: 0x0001CE0F File Offset: 0x0001BE0F
		[InjectMultipleInstances]
		public IMultipleInstancesProvider<IParserService> ParserServiceProvidersProvider { get; private set; }

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06000B6E RID: 2926 RVA: 0x0001CE18 File Offset: 0x0001BE18
		// (set) Token: 0x06000B6F RID: 2927 RVA: 0x0001CE20 File Offset: 0x0001BE20
		[InjectMultipleInstances]
		public IMultipleInstancesProvider<IScannerService> ScannerServiceProvidersProvider { get; private set; }

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06000B70 RID: 2928 RVA: 0x0001CE29 File Offset: 0x0001BE29
		// (set) Token: 0x06000B71 RID: 2929 RVA: 0x0001CE31 File Offset: 0x0001BE31
		[InjectSingleInstance(Shared = true)]
		public ISharedSingleInstanceProvider<_IScannerParserProvider> ScannerParserProviderProvider { get; private set; }
	}
}
