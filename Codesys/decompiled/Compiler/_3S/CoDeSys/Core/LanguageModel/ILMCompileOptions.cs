using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompileOptions
	{
		bool ReplaceConstants { get; set; }

		bool UnicodeIdentifiers { get; set; }

		int MaxCompilerWarnings { get; set; }

		bool EnableBreakpointLogging { get; set; }
	}
}
