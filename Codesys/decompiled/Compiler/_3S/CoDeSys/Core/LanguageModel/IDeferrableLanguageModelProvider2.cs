using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDeferrableLanguageModelProvider2 : IDeferrableLanguageModelProvider
	{
		bool DeferLanguageModelProvision(bool duringDeferedUpdateLanguageModel);
	}
}
