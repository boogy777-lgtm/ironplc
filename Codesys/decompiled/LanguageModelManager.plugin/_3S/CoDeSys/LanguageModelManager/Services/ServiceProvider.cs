using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Compiler.LanguageModelServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.Interfaces;
using _3S.CoDeSys.LanguageModelManager.LMCompiledSetOperations;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000251 RID: 593
	[TypeGuid("{17EF6187-9C66-414E-A9C6-AC96BBE9D407}")]
	[SystemInterface("_3S.CoDeSys.Core.LanguageModel.ILMServiceProvider")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class ServiceProvider : _ILMServiceProvider3, ILMServiceProvider5, ILMServiceProvider4, ILMServiceProvider3, ILMServiceProvider2, ILMServiceProvider, ISystemInstanceRequiresInitialization
	{
		// Token: 0x060027BE RID: 10174 RVA: 0x00063AE4 File Offset: 0x00062AE4
		public ServiceProvider()
		{
			this.CompiledSetStorage = new CompiledSetStorage();
			this.LMPouSetPersistenceService = new LMCompiledSetPersistence();
			this.DownloadedApplicationService = new DownloadedApplicationService();
			this.ConfigurationService = new ConfigurationService();
			this.CreatorService = new CreatorService();
			this.MonitoringService = new MonitoringService();
			this.FlowMonitoringService = new FlowMonitoringService();
			this.PreCompileCrossReferenceService = new PreCompileCrossReferenceService();
			this.PreCompileSmartCodingService = new PreCompileSmartCodingService();
			this.PreCompileStorageSizeEstimatorService = new PreCompileStorageSizeEstimatorService();
			this.CompiledSetStorageFormatFactory = new ArchiveStorageFormatFactory();
			this.ObsoleteService = new ObsoleteService();
			this.TaskLMService = new TaskLMService();
		}

		// Token: 0x060027BF RID: 10175 RVA: 0x00063B86 File Offset: 0x00062B86
		public ServiceProvider(LanguageModelManagerConsolidated lmm) : this()
		{
			this._Lmm = lmm;
		}

		// Token: 0x17000AFF RID: 2815
		// (get) Token: 0x060027C0 RID: 10176 RVA: 0x00063B95 File Offset: 0x00062B95
		private LanguageModelManagerConsolidated Lmm
		{
			get
			{
				if (this._Lmm == null)
				{
					this._Lmm = APEnvironment.LanguageModelMgr;
				}
				return this._Lmm;
			}
		}

		// Token: 0x17000B00 RID: 2816
		// (get) Token: 0x060027C1 RID: 10177 RVA: 0x00063BB0 File Offset: 0x00062BB0
		public ILMCommandService CommandService
		{
			get
			{
				return this._CommandService = (this._CommandService ?? new CommandService(this.Lmm));
			}
		}

		// Token: 0x17000B01 RID: 2817
		// (get) Token: 0x060027C2 RID: 10178 RVA: 0x00063BDC File Offset: 0x00062BDC
		public ILMCompileService CompileService
		{
			get
			{
				return this._CompileService = (this._CompileService ?? new CompileService(this.Lmm));
			}
		}

		// Token: 0x17000B02 RID: 2818
		// (get) Token: 0x060027C3 RID: 10179 RVA: 0x00063C07 File Offset: 0x00062C07
		public ILMConfigurationService ConfigurationService { get; }

		// Token: 0x17000B03 RID: 2819
		// (get) Token: 0x060027C4 RID: 10180 RVA: 0x00063C0F File Offset: 0x00062C0F
		public ILMCreatorService CreatorService { get; }

		// Token: 0x17000B04 RID: 2820
		// (get) Token: 0x060027C5 RID: 10181 RVA: 0x00063C17 File Offset: 0x00062C17
		public ILMDownloadedApplicationService DownloadedApplicationService { get; }

		// Token: 0x17000B05 RID: 2821
		// (get) Token: 0x060027C6 RID: 10182 RVA: 0x00063C1F File Offset: 0x00062C1F
		public ILMFlowMonitoringService FlowMonitoringService { get; }

		// Token: 0x17000B06 RID: 2822
		// (get) Token: 0x060027C7 RID: 10183 RVA: 0x00063C28 File Offset: 0x00062C28
		public ILMProviderService LanguageModelProviderService
		{
			get
			{
				return this._LanguageModelProviderService = (this._LanguageModelProviderService ?? new LanguageModelProviderService(this.Lmm));
			}
		}

		// Token: 0x17000B07 RID: 2823
		// (get) Token: 0x060027C8 RID: 10184 RVA: 0x00063C53 File Offset: 0x00062C53
		public ILMMonitoringService MonitoringService { get; }

		// Token: 0x17000B08 RID: 2824
		// (get) Token: 0x060027C9 RID: 10185 RVA: 0x00063C5B File Offset: 0x00062C5B
		public ILMObsoleteService ObsoleteService { get; }

		// Token: 0x17000B09 RID: 2825
		// (get) Token: 0x060027CA RID: 10186 RVA: 0x00063C63 File Offset: 0x00062C63
		// (set) Token: 0x060027CB RID: 10187 RVA: 0x00063C6B File Offset: 0x00062C6B
		public ILMPreCompileCrossReferenceService PreCompileCrossReferenceService { get; private set; }

		// Token: 0x17000B0A RID: 2826
		// (get) Token: 0x060027CC RID: 10188 RVA: 0x00063C74 File Offset: 0x00062C74
		public PreCompileService PreCompileService
		{
			get
			{
				return this._PreCompileService = (this._PreCompileService ?? new PreCompileService(this.Lmm));
			}
		}

		// Token: 0x17000B0B RID: 2827
		// (get) Token: 0x060027CD RID: 10189 RVA: 0x00063C9F File Offset: 0x00062C9F
		ILMPreCompileService ILMServiceProvider.PreCompileService
		{
			get
			{
				return this.PreCompileService;
			}
		}

		// Token: 0x17000B0C RID: 2828
		// (get) Token: 0x060027CE RID: 10190 RVA: 0x00063CA7 File Offset: 0x00062CA7
		public ILMPreCompileSmartCodingService PreCompileSmartCodingService { get; }

		// Token: 0x17000B0D RID: 2829
		// (get) Token: 0x060027CF RID: 10191 RVA: 0x00063CAF File Offset: 0x00062CAF
		public ILMPreCompileStorageSizeEstimatorService PreCompileStorageSizeEstimatorService { get; }

		// Token: 0x17000B0E RID: 2830
		// (get) Token: 0x060027D0 RID: 10192 RVA: 0x00063CB7 File Offset: 0x00062CB7
		public ILMCompiledSetArchiveStorageFormatFactory CompiledSetStorageFormatFactory { get; }

		// Token: 0x17000B0F RID: 2831
		// (get) Token: 0x060027D1 RID: 10193 RVA: 0x00063CBF File Offset: 0x00062CBF
		public ILMCompiledSetStorage CompiledSetStorage { get; }

		// Token: 0x17000B10 RID: 2832
		// (get) Token: 0x060027D2 RID: 10194 RVA: 0x00063CC7 File Offset: 0x00062CC7
		public ILMPouSetPersistenceService LMPouSetPersistenceService { get; }

		// Token: 0x17000B11 RID: 2833
		// (get) Token: 0x060027D3 RID: 10195 RVA: 0x00063CCF File Offset: 0x00062CCF
		public ILMCachingService CachingService
		{
			get
			{
				return (ILMCachingService)this.CommandService;
			}
		}

		// Token: 0x17000B12 RID: 2834
		// (get) Token: 0x060027D4 RID: 10196 RVA: 0x00063CDC File Offset: 0x00062CDC
		public ITaskLMService TaskLMService { get; }

		// Token: 0x17000B13 RID: 2835
		// (get) Token: 0x060027D5 RID: 10197 RVA: 0x00063CE4 File Offset: 0x00062CE4
		public ISingleByteStringEncodingService SingleByteStringEncodingService
		{
			get
			{
				return VersionedCompilerFactory._SingleByteStringEncodingService;
			}
		}

		// Token: 0x17000B14 RID: 2836
		// (get) Token: 0x060027D6 RID: 10198 RVA: 0x00063CEB File Offset: 0x00062CEB
		public ILMCallTreeService CallTreeService
		{
			get
			{
				return VersionedCompilerFactory._CallTreeService_OrNull;
			}
		}

		// Token: 0x17000B15 RID: 2837
		// (get) Token: 0x060027D7 RID: 10199 RVA: 0x00063CF2 File Offset: 0x00062CF2
		public ILMNameManglingService NameManglingService
		{
			get
			{
				return VersionedCompilerFactory._NameManglingService_OrNull;
			}
		}

		// Token: 0x17000B16 RID: 2838
		// (get) Token: 0x060027D8 RID: 10200 RVA: 0x00063CF9 File Offset: 0x00062CF9
		public ILMTransitionUserCodeAnalyzerService TransitionUserCodeAnalyzerService
		{
			get
			{
				return VersionedCompilerFactory._TransitionUserCodeAnalyzerService_OrNull;
			}
		}

		// Token: 0x060027D9 RID: 10201 RVA: 0x00063D00 File Offset: 0x00062D00
		public void OnAllSystemInstancesAvailable()
		{
			this.PreCompileService.OnAllSystemInstancesAvailable();
		}

		// Token: 0x0400076E RID: 1902
		private LanguageModelManagerConsolidated _Lmm;

		// Token: 0x0400076F RID: 1903
		private ILMCommandService _CommandService;

		// Token: 0x04000770 RID: 1904
		private ILMCompileService _CompileService;

		// Token: 0x04000775 RID: 1909
		private ILMProviderService _LanguageModelProviderService;

		// Token: 0x04000779 RID: 1913
		private PreCompileService _PreCompileService;
	}
}
