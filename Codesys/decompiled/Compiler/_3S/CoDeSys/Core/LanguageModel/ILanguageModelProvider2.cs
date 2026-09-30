using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelProvider2 : ILanguageModelProvider
	{
		bool NeedsContextForLanguageModelProvision { get; }
	}
}
