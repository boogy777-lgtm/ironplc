using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryTable4 : ILibraryTable3, ILibraryTable2, ILibraryTable
	{
		bool? GetQualifiedOnlyRecursive(IPreCompileContext precomLocal, string stLibraryIdToLookup);

		string GetLocalLibraryNamespaceRecursive(IPreCompileContext precomLocal, string stLibraryToFind);
	}
}
