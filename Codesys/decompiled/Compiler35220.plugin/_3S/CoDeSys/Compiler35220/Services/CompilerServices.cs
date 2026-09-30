using System;
using \u0002;
using \u0006;
using \u000E;
using \u0012;
using \u0014;
using \u0016;
using \u0017;
using \u0019;
using \u001A;
using \u001D;
using \u001E;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services.AttributeCheck;
using _3S.CoDeSys.Compiler35220.Services.PreCompileSizeCalculation;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000D7 RID: 215
	[TypeGuid("{22222222-7c1d-43ad-8e3b-d51f7e195444}")]
	public class CompilerServices : ICompilerServiceFactory, ICompilerServiceGreenTreeConverterFactory, ICompilerSerializationServiceFactory, ICompilerComparisonServiceFactory, ICompilerServiceExprementWriterFactory, IConstantFoldingFactory, IAddressCalculatorFactory, IParseTreeService, IStringEncodingServiceFactory, IUpToDateCheckerFactory, IQualifierServiceFactory, ITypeServiceFactory, ILMStringEncodingServiceFactory, ILMCallTreeServiceFactory, IPreCompileSizeCalculatorFactory, IGranularityCalculatorFactory, ILMAttributeProviderFactory, ILMNameManglingServiceFactory, ILMTransitionUserCodeAnalyzerServiceFactory
	{
		// Token: 0x06000F15 RID: 3861 RVA: 0x00029728 File Offset: 0x00027928
		public ICompiler CreateCompiler()
		{
			return new CompilerServicesInternal();
		}

		// Token: 0x06000F16 RID: 3862 RVA: 0x00029730 File Offset: 0x00027930
		public ICompilerHelper CreateHelper()
		{
			return new Helper();
		}

		// Token: 0x06000F17 RID: 3863 RVA: 0x00029738 File Offset: 0x00027938
		public ITypeComparer CreateTypeComparer()
		{
			return new global::\u0006.\u0011();
		}

		// Token: 0x06000F18 RID: 3864 RVA: 0x00029740 File Offset: 0x00027940
		public ILanguageModelHandling CreateLMHandler()
		{
			return new \u001D.\u0004();
		}

		// Token: 0x06000F19 RID: 3865 RVA: 0x00029748 File Offset: 0x00027948
		public ITypeCompiler CreateTypeCompiler()
		{
			return new TypeCompiler();
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x00029750 File Offset: 0x00027950
		public IGreenTreeConverter CreateGreenTreeConverter()
		{
			return new \u001E.\u0001();
		}

		// Token: 0x06000F1B RID: 3867 RVA: 0x00029758 File Offset: 0x00027958
		public ILMSerializationService CreateSerializationService()
		{
			return new \u001E.\u0001();
		}

		// Token: 0x06000F1C RID: 3868 RVA: 0x00029760 File Offset: 0x00027960
		public IParseTreeProvider CreateParseTreeProvider(_ICompiledPOU2 cpou, IGreenTreeConverter converter, ITreeFactory treeFactory)
		{
			return new global::\u0002.\u0003(cpou, converter, treeFactory);
		}

		// Token: 0x06000F1D RID: 3869 RVA: 0x0002976C File Offset: 0x0002796C
		public IComparisonService CreateComparisonService()
		{
			return new ComparisonService();
		}

		// Token: 0x06000F1E RID: 3870 RVA: 0x00029774 File Offset: 0x00027974
		public ICompilerServiceExprementWriter CreateExprementWriterService()
		{
			return new global::\u0012.\u0003();
		}

		// Token: 0x06000F1F RID: 3871 RVA: 0x0002977C File Offset: 0x0002797C
		public IConstantFolder CreateConstantFolder()
		{
			return new global::\u0002.\u0002();
		}

		// Token: 0x17000472 RID: 1138
		// (get) Token: 0x06000F20 RID: 3872 RVA: 0x00029784 File Offset: 0x00027984
		public _ICompilerMessageCreator CompilerMessageCreator
		{
			get
			{
				return global::\u0019.\u0014.Instance;
			}
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x0002978C File Offset: 0x0002798C
		public IAddressCalculator CreateAddressCalculator()
		{
			return new AddressCalculator();
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x00029794 File Offset: 0x00027994
		public ILMCachingService CreateCachingService()
		{
			return new CompilerServices() as ILMCachingService;
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x000297A0 File Offset: 0x000279A0
		public IStringEncodingService CreateStringEncodingService()
		{
			return global::\u0017.\u0003.Singleton;
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x000297A8 File Offset: 0x000279A8
		public IUpToDateChecker CreateUpToDateChecker()
		{
			return new global::\u0014.\u0004();
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x000297B0 File Offset: 0x000279B0
		public ILMQualifierService CreateQualifierService()
		{
			return new global::\u000E.\u0003();
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x000297B8 File Offset: 0x000279B8
		public ILMTypeService CreateTypeService()
		{
			return TypeTableClass.Singleton;
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x000297C0 File Offset: 0x000279C0
		public ILMStringEncodingService CreateLMStringEncodingService()
		{
			return global::\u0017.\u0003.Singleton;
		}

		// Token: 0x06000F28 RID: 3880 RVA: 0x000297C8 File Offset: 0x000279C8
		public ILMCallTreeService CreateLMCallTreeService()
		{
			return new global::\u001A.\u0003();
		}

		// Token: 0x06000F29 RID: 3881 RVA: 0x000297D0 File Offset: 0x000279D0
		public IPreCompileSizeCalculator CreatePreCompileSizeCalculator()
		{
			return new PreCompileSizeCalculator();
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x000297D8 File Offset: 0x000279D8
		public IGranularityCalculator CreateGranularityCalculator()
		{
			return new GranularityCalculator();
		}

		// Token: 0x06000F2B RID: 3883 RVA: 0x000297E0 File Offset: 0x000279E0
		public ILMAttributeProvider CreateLMAttributeProvider()
		{
			return new LMMAttributeProvider();
		}

		// Token: 0x06000F2C RID: 3884 RVA: 0x000297E8 File Offset: 0x000279E8
		public ILMNameManglingService CreateLMNameManglingService()
		{
			return NameManglingService.Instance;
		}

		// Token: 0x06000F2D RID: 3885 RVA: 0x000297F0 File Offset: 0x000279F0
		public ILMTransitionUserCodeAnalyzerService CreateLMTransitionUserCodeAnalyzerService()
		{
			return global::\u0016.\u0006.Singleton;
		}
	}
}
