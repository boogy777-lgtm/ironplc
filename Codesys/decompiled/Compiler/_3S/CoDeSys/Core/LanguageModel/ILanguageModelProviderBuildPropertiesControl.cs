using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelProviderBuildPropertiesControl : ILanguageModelProvider
	{
		bool ShowPropertiesDialog { get; }

		bool ExcludeFromBuildEnabled { get; }

		bool ExternalEnabled { get; }

		bool EnableSystemCallEnabled { get; }

		bool LinkAlwaysEnabled { get; }

		bool CompilerDefinesEnabled { get; }
	}
}
