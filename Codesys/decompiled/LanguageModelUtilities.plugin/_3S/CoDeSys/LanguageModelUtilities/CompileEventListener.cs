using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelUtilities.RetainsUpdatedInCycleReporter;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[SystemInterface("_3S.CoDeSys.LanguageModelUtilities.CompileEventListener")]
	[TypeGuid("{174185C5-14B7-4D1D-849E-668D09DA041D}")]
	public class CompileEventListener : ISystemInstanceRequiresInitialization
	{
		public void OnAllSystemInstancesAvailable()
		{
			RegisterAnalyzationLateLm(APEnvironmentFacade.Instance.LanguageModelMgr);
			RegisterLateLmReporterForRetainPersistentVars(APEnvironmentFacade.Instance.LanguageModelMgr);
		}

		private void RegisterAnalyzationLateLm(ILanguageModelManager22 instanceLanguageModelMgr)
		{
			instanceLanguageModelMgr.AfterCompile += OnAfterCompile;
		}

		public void RegisterLateLmReporterForRetainPersistentVars(ILanguageModelManager22 instanceLanguageModelMgr)
		{
			instanceLanguageModelMgr.AddLateLanguageModel += OnAddLateLanguageModel;
		}

		private void OnAfterCompile(object sender, CompileEventArgs e)
		{
			AnalyzationService.AddInstrumentationForAnalyzation(e);
		}

		private void OnAddLateLanguageModel(object sender, AddLanguageModelEventArgs e)
		{
			ReportWrittenRetainsAndPersistentVars(e);
		}

		private void ReportWrittenRetainsAndPersistentVars(AddLanguageModelEventArgs e)
		{
			if (APEnvironmentFacade.Instance.ReportRetainPersistentUpdateInCycleEnabled(e.ApplicationGuid))
			{
				new RetainsUpdatedInCycleReportingCodeGenerator(e.ApplicationGuid, e.LanguageModelList).GenerateCode();
			}
		}
	}
}
