using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ILanguageModelManagerTargetSettings3 : ILanguageModelManagerTargetSettings2, ILanguageModelManagerTargetSettings
	{
		IRegisteredTargetSetting SingleOnlineChangeArea { get; }
	}
}
