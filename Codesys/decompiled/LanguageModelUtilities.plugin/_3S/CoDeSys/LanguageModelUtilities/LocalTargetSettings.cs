using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{209E36E7-C048-4FF4-B8C9-F1096FEF7523}")]
	public class LocalTargetSettings : ITargetSettingsUser
	{
		internal static IRegisteredTargetSetting RuntimeVersion => APEnvironmentFacade.Instance.GetTargetSetting("runtime_identification\\version");

		internal static IRegisteredTargetSetting ReportRetainPersistentUpdateInCycle => APEnvironmentFacade.Instance.GetTargetSetting("codegenerator\\report-retain-persistent-update-in-cycle");
	}
}
