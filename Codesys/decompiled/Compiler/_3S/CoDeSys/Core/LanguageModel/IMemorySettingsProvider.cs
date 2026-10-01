using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMemorySettingsProvider
	{
		void UpdateMemorySettings(IMemorySettings memorySettings);
	}
}
