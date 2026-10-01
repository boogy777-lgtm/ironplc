using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C5 RID: 197
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Further reduction of class coupling will lead to unreasonably splitting this class")]
	public class VersionedCompilerFactory
	{
		// Token: 0x06000C0F RID: 3087 RVA: 0x00002476 File Offset: 0x00001476
		protected VersionedCompilerFactory()
		{
		}

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x06000C10 RID: 3088 RVA: 0x0001EE80 File Offset: 0x0001DE80
		// (set) Token: 0x06000C11 RID: 3089 RVA: 0x0001EE87 File Offset: 0x0001DE87
		public static bool FirstLanguageModel { get; set; } = true;

		// Token: 0x06000C12 RID: 3090 RVA: 0x0001EE90 File Offset: 0x0001DE90
		internal static void Reset()
		{
			VersionedCompilerFactory.s_compiler = null;
			VersionedCompilerFactory.s_helper = null;
			VersionedCompilerFactory.s_comparisonService = null;
			VersionedCompilerFactory.s_typecomp = null;
			VersionedCompilerFactory.s_lmmhandler = null;
			VersionedCompilerFactory.s_typecompiler = null;
			VersionedCompilerFactory.s_compilerMessageCreator = null;
			VersionedCompilerFactory.s_greentreeconverter = null;
			VersionedCompilerFactory.s_parsetreeservice = null;
			VersionedCompilerFactory.s_serializationservice = null;
			VersionedCompilerFactory.s_ExprementWriter = null;
			VersionedCompilerFactory.s_ConstantFolder = null;
			VersionedCompilerFactory.s_AddressCalculator = null;
			VersionedCompilerFactory.s_StringEncoding = null;
			VersionedCompilerFactory.s_UpToDateChecker = null;
			VersionedCompilerFactory.s_QualifierService = null;
			VersionedCompilerFactory.s_TypeService = null;
			VersionedCompilerFactory.ResetScannerParserProvider();
			VersionedCompilerFactory.s_PreCompileSizeCalculator = null;
			VersionedCompilerFactory.s_GranularityCalculator = null;
			VersionedCompilerFactory.s_LMAttributeProvider = null;
			VersionedCompilerFactory.s_NameManglingService = null;
			VersionedCompilerFactory.s_TransitionUserCodeAnalyzerService = null;
			AttributeManagerX.Singleton.Reset();
			VersionedCompilerFactory.FirstLanguageModel = true;
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x0001EF36 File Offset: 0x0001DF36
		internal static void ResetScannerParserProvider()
		{
			((ScannerParserProvider)APEnvironmentFacade.Instance.ScannerParserProvider).Reset();
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x0001EF4C File Offset: 0x0001DF4C
		private static ICompilerServiceFactory FindFactoryForRequestedCompilerVersion(Version requestedVersion)
		{
			foreach (_ICompilerVersionsProvider icompilerVersionsProvider in APEnvironmentFacade.Instance.CreateCompilerVersionsProviders())
			{
				if (icompilerVersionsProvider.ProvidesVersion(requestedVersion))
				{
					return icompilerVersionsProvider.ServiceFactory;
				}
			}
			return null;
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x0001EFAC File Offset: 0x0001DFAC
		private static ICompilerServiceFactory FindFactoryForCompilerVersion()
		{
			if (!Common.IsLibraryWithPinnedStorageVersion())
			{
				ICompilerServiceFactory compilerServiceFactory = VersionedCompilerFactory.FindFactoryForRequestedCompilerVersion(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse());
				if (compilerServiceFactory != null)
				{
					return compilerServiceFactory;
				}
			}
			return VersionedCompilerFactory.FindFactoryForRequestedCompilerVersion((from v in APEnvironmentFacade.Instance.CompilerVersionMgr.AvailableCompilerVersionsOEMFilteredNotReplaced
			orderby v descending
			select v).First<Version>());
		}

		// Token: 0x06000C16 RID: 3094 RVA: 0x0001F018 File Offset: 0x0001E018
		internal static ICompiler GetCompilerOrNull(Version version)
		{
			ICompilerServiceFactory compilerServiceFactory = VersionedCompilerFactory.FindFactoryForRequestedCompilerVersion(version);
			if (compilerServiceFactory == null)
			{
				return null;
			}
			return compilerServiceFactory.CreateCompiler();
		}

		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06000C17 RID: 3095 RVA: 0x0001F037 File Offset: 0x0001E037
		internal static ICompiler _Compiler
		{
			get
			{
				if (VersionedCompilerFactory.s_compiler == null)
				{
					VersionedCompilerFactory.s_compiler = VersionedCompilerFactory.FindFactoryForCompilerVersion().CreateCompiler();
				}
				return VersionedCompilerFactory.s_compiler;
			}
		}

		// Token: 0x170002FC RID: 764
		// (get) Token: 0x06000C18 RID: 3096 RVA: 0x0001F054 File Offset: 0x0001E054
		internal static ICompilerHelper _Helper
		{
			get
			{
				if (VersionedCompilerFactory.s_helper == null)
				{
					VersionedCompilerFactory.s_helper = VersionedCompilerFactory.FindFactoryForCompilerVersion().CreateHelper();
				}
				return VersionedCompilerFactory.s_helper;
			}
		}

		// Token: 0x170002FD RID: 765
		// (get) Token: 0x06000C19 RID: 3097 RVA: 0x0001F071 File Offset: 0x0001E071
		internal static ITypeComparer _TypeComparer
		{
			get
			{
				if (VersionedCompilerFactory.s_typecomp == null)
				{
					VersionedCompilerFactory.s_typecomp = VersionedCompilerFactory.FindFactoryForCompilerVersion().CreateTypeComparer();
				}
				return VersionedCompilerFactory.s_typecomp;
			}
		}

		// Token: 0x170002FE RID: 766
		// (get) Token: 0x06000C1A RID: 3098 RVA: 0x0001F08E File Offset: 0x0001E08E
		internal static ITypeCompiler _TypeCompiler
		{
			get
			{
				if (VersionedCompilerFactory.s_typecompiler == null)
				{
					VersionedCompilerFactory.s_typecompiler = VersionedCompilerFactory.FindFactoryForCompilerVersion().CreateTypeCompiler();
				}
				return VersionedCompilerFactory.s_typecompiler;
			}
		}

		// Token: 0x170002FF RID: 767
		// (get) Token: 0x06000C1B RID: 3099 RVA: 0x0001F0AB File Offset: 0x0001E0AB
		internal static _ICompilerMessageCreator _CompilerMessageCreator
		{
			get
			{
				if (VersionedCompilerFactory.s_compilerMessageCreator == null)
				{
					VersionedCompilerFactory.s_compilerMessageCreator = VersionedCompilerFactory.FindFactoryForCompilerVersion().CompilerMessageCreator;
				}
				return VersionedCompilerFactory.s_compilerMessageCreator;
			}
		}

		// Token: 0x17000300 RID: 768
		// (get) Token: 0x06000C1C RID: 3100 RVA: 0x0001F0C8 File Offset: 0x0001E0C8
		internal static ILanguageModelHandling _LMMHandler
		{
			get
			{
				if (VersionedCompilerFactory.s_lmmhandler == null)
				{
					VersionedCompilerFactory.s_lmmhandler = VersionedCompilerFactory.FindFactoryForCompilerVersion().CreateLMHandler();
				}
				return VersionedCompilerFactory.s_lmmhandler;
			}
		}

		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06000C1D RID: 3101 RVA: 0x0001F0E8 File Offset: 0x0001E0E8
		internal static IGreenTreeConverter _GreenTreeConverter_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_greentreeconverter == null)
				{
					ICompilerServiceFactory compilerServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion();
					if (compilerServiceFactory is ICompilerServiceGreenTreeConverterFactory)
					{
						VersionedCompilerFactory.s_greentreeconverter = (compilerServiceFactory as ICompilerServiceGreenTreeConverterFactory).CreateGreenTreeConverter();
					}
				}
				return VersionedCompilerFactory.s_greentreeconverter;
			}
		}

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06000C1E RID: 3102 RVA: 0x0001F120 File Offset: 0x0001E120
		internal static IParseTreeService ParseTreeService
		{
			get
			{
				if (VersionedCompilerFactory.s_parsetreeservice == null)
				{
					ICompilerServiceFactory compilerServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion();
					if (compilerServiceFactory is IParseTreeService)
					{
						VersionedCompilerFactory.s_parsetreeservice = (compilerServiceFactory as IParseTreeService);
					}
					else
					{
						VersionedCompilerFactory.s_parsetreeservice = new DefaultParseTreeService();
					}
				}
				return VersionedCompilerFactory.s_parsetreeservice;
			}
		}

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x06000C1F RID: 3103 RVA: 0x0001F160 File Offset: 0x0001E160
		internal static ILMSerializationService _SerializationService_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_serializationservice == null)
				{
					ICompilerServiceFactory compilerServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion();
					if (compilerServiceFactory is ICompilerSerializationServiceFactory)
					{
						VersionedCompilerFactory.s_serializationservice = (compilerServiceFactory as ICompilerSerializationServiceFactory).CreateSerializationService();
					}
				}
				return VersionedCompilerFactory.s_serializationservice;
			}
		}

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06000C20 RID: 3104 RVA: 0x0001F198 File Offset: 0x0001E198
		internal static IConstantFolder _ConstantFolder_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_ConstantFolder == null)
				{
					ICompilerServiceFactory compilerServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion();
					if (compilerServiceFactory is IConstantFoldingFactory)
					{
						VersionedCompilerFactory.s_ConstantFolder = (compilerServiceFactory as IConstantFoldingFactory).CreateConstantFolder();
					}
				}
				return VersionedCompilerFactory.s_ConstantFolder;
			}
		}

		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06000C21 RID: 3105 RVA: 0x0001F1CF File Offset: 0x0001E1CF
		internal static IUpToDateChecker _UpToDateChecker_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_UpToDateChecker == null)
				{
					IUpToDateCheckerFactory upToDateCheckerFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as IUpToDateCheckerFactory;
					VersionedCompilerFactory.s_UpToDateChecker = ((upToDateCheckerFactory != null) ? upToDateCheckerFactory.CreateUpToDateChecker() : null);
				}
				return VersionedCompilerFactory.s_UpToDateChecker;
			}
		}

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06000C22 RID: 3106 RVA: 0x0001F1F8 File Offset: 0x0001E1F8
		internal static ILMTypeService _TypeService_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_TypeService == null)
				{
					ITypeServiceFactory typeServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as ITypeServiceFactory;
					VersionedCompilerFactory.s_TypeService = ((typeServiceFactory != null) ? typeServiceFactory.CreateTypeService() : null);
				}
				return VersionedCompilerFactory.s_TypeService;
			}
		}

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000C23 RID: 3107 RVA: 0x0001F221 File Offset: 0x0001E221
		internal static ILMStringEncodingService _StringEncodingService_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_StringEncodingService == null)
				{
					ILMStringEncodingServiceFactory ilmstringEncodingServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as ILMStringEncodingServiceFactory;
					VersionedCompilerFactory.s_StringEncodingService = ((ilmstringEncodingServiceFactory != null) ? ilmstringEncodingServiceFactory.CreateLMStringEncodingService() : null);
				}
				return VersionedCompilerFactory.s_StringEncodingService;
			}
		}

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06000C24 RID: 3108 RVA: 0x0001F24A File Offset: 0x0001E24A
		internal static ILMCallTreeService _CallTreeService_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_CallTreeService == null)
				{
					ILMCallTreeServiceFactory ilmcallTreeServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as ILMCallTreeServiceFactory;
					VersionedCompilerFactory.s_CallTreeService = ((ilmcallTreeServiceFactory != null) ? ilmcallTreeServiceFactory.CreateLMCallTreeService() : null);
				}
				return VersionedCompilerFactory.s_CallTreeService;
			}
		}

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06000C25 RID: 3109 RVA: 0x0001F273 File Offset: 0x0001E273
		internal static ILMNameManglingService _NameManglingService_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_NameManglingService == null)
				{
					ILMNameManglingServiceFactory ilmnameManglingServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as ILMNameManglingServiceFactory;
					VersionedCompilerFactory.s_NameManglingService = ((ilmnameManglingServiceFactory != null) ? ilmnameManglingServiceFactory.CreateLMNameManglingService() : null);
				}
				return VersionedCompilerFactory.s_NameManglingService;
			}
		}

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06000C26 RID: 3110 RVA: 0x0001F29C File Offset: 0x0001E29C
		internal static ILMTransitionUserCodeAnalyzerService _TransitionUserCodeAnalyzerService_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_TransitionUserCodeAnalyzerService == null)
				{
					ILMTransitionUserCodeAnalyzerServiceFactory ilmtransitionUserCodeAnalyzerServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as ILMTransitionUserCodeAnalyzerServiceFactory;
					VersionedCompilerFactory.s_TransitionUserCodeAnalyzerService = ((ilmtransitionUserCodeAnalyzerServiceFactory != null) ? ilmtransitionUserCodeAnalyzerServiceFactory.CreateLMTransitionUserCodeAnalyzerService() : null);
				}
				return VersionedCompilerFactory.s_TransitionUserCodeAnalyzerService;
			}
		}

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x06000C27 RID: 3111 RVA: 0x0001F2C5 File Offset: 0x0001E2C5
		internal static ILMQualifierService _QualifierService_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_QualifierService == null)
				{
					IQualifierServiceFactory qualifierServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as IQualifierServiceFactory;
					VersionedCompilerFactory.s_QualifierService = ((qualifierServiceFactory != null) ? qualifierServiceFactory.CreateQualifierService() : null);
				}
				return VersionedCompilerFactory.s_QualifierService;
			}
		}

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x06000C28 RID: 3112 RVA: 0x0001F2EE File Offset: 0x0001E2EE
		internal static IComparisonService _ComparisonService
		{
			get
			{
				if (VersionedCompilerFactory.s_comparisonService == null)
				{
					VersionedCompilerFactory.s_comparisonService = ((ICompilerComparisonServiceFactory)VersionedCompilerFactory.FindFactoryForCompilerVersion()).CreateComparisonService();
				}
				return VersionedCompilerFactory.s_comparisonService;
			}
		}

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x06000C29 RID: 3113 RVA: 0x0001F310 File Offset: 0x0001E310
		internal static ICompilerServiceExprementWriter _ExprementWriter
		{
			get
			{
				if (VersionedCompilerFactory.s_ExprementWriter == null)
				{
					ICompilerServiceExprementWriterFactory compilerServiceExprementWriterFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as ICompilerServiceExprementWriterFactory;
					if (compilerServiceExprementWriterFactory == null)
					{
						VersionedCompilerFactory.s_ExprementWriter = new DefaultExprementWriter(VersionedCompilerFactory._Compiler);
					}
					else
					{
						VersionedCompilerFactory.s_ExprementWriter = compilerServiceExprementWriterFactory.CreateExprementWriterService();
					}
				}
				return VersionedCompilerFactory.s_ExprementWriter;
			}
		}

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06000C2A RID: 3114 RVA: 0x0001F353 File Offset: 0x0001E353
		internal static IAddressCalculator _AddressCalculator
		{
			get
			{
				if (VersionedCompilerFactory.s_AddressCalculator == null)
				{
					IAddressCalculatorFactory addressCalculatorFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as IAddressCalculatorFactory;
					VersionedCompilerFactory.s_AddressCalculator = ((addressCalculatorFactory != null) ? addressCalculatorFactory.CreateAddressCalculator() : null);
				}
				return VersionedCompilerFactory.s_AddressCalculator;
			}
		}

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06000C2B RID: 3115 RVA: 0x0001F37C File Offset: 0x0001E37C
		internal static IStringEncodingService _StringEncodingService
		{
			get
			{
				if (VersionedCompilerFactory.s_StringEncoding == null)
				{
					IStringEncodingServiceFactory stringEncodingServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as IStringEncodingServiceFactory;
					if (stringEncodingServiceFactory == null)
					{
						VersionedCompilerFactory.s_StringEncoding = new DefaultStringEncodingService();
					}
					else
					{
						VersionedCompilerFactory.s_StringEncoding = stringEncodingServiceFactory.CreateStringEncodingService();
					}
				}
				return VersionedCompilerFactory.s_StringEncoding;
			}
		}

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06000C2C RID: 3116 RVA: 0x0001F3BC File Offset: 0x0001E3BC
		internal static ISingleByteStringEncodingService _SingleByteStringEncodingService
		{
			get
			{
				if (VersionedCompilerFactory.s_StringEncoding == null)
				{
					IStringEncodingServiceFactory stringEncodingServiceFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as IStringEncodingServiceFactory;
					if (stringEncodingServiceFactory == null)
					{
						VersionedCompilerFactory.s_StringEncoding = new DefaultStringEncodingService();
					}
					else
					{
						VersionedCompilerFactory.s_StringEncoding = stringEncodingServiceFactory.CreateStringEncodingService();
					}
				}
				return VersionedCompilerFactory.s_StringEncoding as ISingleByteStringEncodingService;
			}
		}

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06000C2D RID: 3117 RVA: 0x0001F3FF File Offset: 0x0001E3FF
		internal static IPreCompileSizeCalculator _PreCompileSizeCalculator_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_PreCompileSizeCalculator == null)
				{
					IPreCompileSizeCalculatorFactory preCompileSizeCalculatorFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as IPreCompileSizeCalculatorFactory;
					VersionedCompilerFactory.s_PreCompileSizeCalculator = ((preCompileSizeCalculatorFactory != null) ? preCompileSizeCalculatorFactory.CreatePreCompileSizeCalculator() : null);
				}
				return VersionedCompilerFactory.s_PreCompileSizeCalculator;
			}
		}

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06000C2E RID: 3118 RVA: 0x0001F428 File Offset: 0x0001E428
		internal static IGranularityCalculator _GranularityCalculator_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_GranularityCalculator == null)
				{
					IGranularityCalculatorFactory granularityCalculatorFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as IGranularityCalculatorFactory;
					VersionedCompilerFactory.s_GranularityCalculator = ((granularityCalculatorFactory != null) ? granularityCalculatorFactory.CreateGranularityCalculator() : null);
				}
				return VersionedCompilerFactory.s_GranularityCalculator;
			}
		}

		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06000C2F RID: 3119 RVA: 0x0001F451 File Offset: 0x0001E451
		internal static ILMAttributeProvider _LMAttributeProvider_OrNull
		{
			get
			{
				if (VersionedCompilerFactory.s_LMAttributeProvider == null)
				{
					ILMAttributeProviderFactory ilmattributeProviderFactory = VersionedCompilerFactory.FindFactoryForCompilerVersion() as ILMAttributeProviderFactory;
					VersionedCompilerFactory.s_LMAttributeProvider = ((ilmattributeProviderFactory != null) ? ilmattributeProviderFactory.CreateLMAttributeProvider() : null);
				}
				return VersionedCompilerFactory.s_LMAttributeProvider;
			}
		}

		// Token: 0x04000214 RID: 532
		private static ICompiler s_compiler = null;

		// Token: 0x04000215 RID: 533
		private static ICompilerHelper s_helper = null;

		// Token: 0x04000216 RID: 534
		private static IComparisonService s_comparisonService = null;

		// Token: 0x04000217 RID: 535
		private static ITypeComparer s_typecomp = null;

		// Token: 0x04000218 RID: 536
		private static ILanguageModelHandling s_lmmhandler = null;

		// Token: 0x04000219 RID: 537
		private static ITypeCompiler s_typecompiler = null;

		// Token: 0x0400021A RID: 538
		private static _ICompilerMessageCreator s_compilerMessageCreator = null;

		// Token: 0x0400021B RID: 539
		private static IGreenTreeConverter s_greentreeconverter = null;

		// Token: 0x0400021C RID: 540
		private static IParseTreeService s_parsetreeservice = null;

		// Token: 0x0400021D RID: 541
		private static ILMSerializationService s_serializationservice = null;

		// Token: 0x0400021E RID: 542
		private static ICompilerServiceExprementWriter s_ExprementWriter = null;

		// Token: 0x0400021F RID: 543
		private static IConstantFolder s_ConstantFolder = null;

		// Token: 0x04000220 RID: 544
		private static IAddressCalculator s_AddressCalculator = null;

		// Token: 0x04000221 RID: 545
		private static IStringEncodingService s_StringEncoding = null;

		// Token: 0x04000222 RID: 546
		private static IUpToDateChecker s_UpToDateChecker = null;

		// Token: 0x04000223 RID: 547
		private static ILMQualifierService s_QualifierService = null;

		// Token: 0x04000224 RID: 548
		private static ILMTypeService s_TypeService = null;

		// Token: 0x04000225 RID: 549
		private static ILMStringEncodingService s_StringEncodingService = null;

		// Token: 0x04000226 RID: 550
		private static ILMCallTreeService s_CallTreeService = null;

		// Token: 0x04000227 RID: 551
		private static IPreCompileSizeCalculator s_PreCompileSizeCalculator = null;

		// Token: 0x04000228 RID: 552
		private static IGranularityCalculator s_GranularityCalculator = null;

		// Token: 0x04000229 RID: 553
		private static ILMAttributeProvider s_LMAttributeProvider = null;

		// Token: 0x0400022A RID: 554
		private static ILMNameManglingService s_NameManglingService = null;

		// Token: 0x0400022B RID: 555
		private static ILMTransitionUserCodeAnalyzerService s_TransitionUserCodeAnalyzerService = null;
	}
}
