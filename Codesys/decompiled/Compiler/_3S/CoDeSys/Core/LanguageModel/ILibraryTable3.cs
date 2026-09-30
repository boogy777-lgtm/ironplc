using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryTable3 : ILibraryTable2, ILibraryTable
	{
		bool GetQualifiedOnly(IPreCompileContext precomLocal, string stLibraryIdToLookup);
	}
}
