using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExternalPluginFoundIssue
	{
		ISourcePosition Position { get; }

		Severity Severity { get; }
	}
}
