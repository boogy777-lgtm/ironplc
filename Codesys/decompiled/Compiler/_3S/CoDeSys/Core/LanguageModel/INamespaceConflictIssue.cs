using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface INamespaceConflictIssue : IExternalPluginFoundIssue
	{
		string Namespace { get; }

		string Library { get; }
	}
}
