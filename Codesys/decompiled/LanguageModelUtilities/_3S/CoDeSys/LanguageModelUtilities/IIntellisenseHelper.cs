using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IIntellisenseHelper
	{
		bool ContainsDeclaration(string stDeclarationSnippet);

		string DetermineAccessPathInCaseOfStructureInitialization(string stDeclarationSnippet);
	}
}
