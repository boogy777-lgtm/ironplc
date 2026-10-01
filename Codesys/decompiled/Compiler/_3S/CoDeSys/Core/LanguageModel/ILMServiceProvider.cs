using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMServiceProvider
	{
		ILMCommandService CommandService { get; }

		ILMConfigurationService ConfigurationService { get; }

		ILMCreatorService CreatorService { get; }

		ILMProviderService LanguageModelProviderService { get; }

		ILMCompileService CompileService { get; }

		ILMDownloadedApplicationService DownloadedApplicationService { get; }

		ILMPreCompileService PreCompileService { get; }

		ILMPreCompileCrossReferenceService PreCompileCrossReferenceService { get; }

		ILMPreCompileSmartCodingService PreCompileSmartCodingService { get; }

		ILMPreCompileStorageSizeEstimatorService PreCompileStorageSizeEstimatorService { get; }

		ILMMonitoringService MonitoringService { get; }

		ILMFlowMonitoringService FlowMonitoringService { get; }

		ILMObsoleteService ObsoleteService { get; }
	}
}
