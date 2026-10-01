using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILibraryTable2 : _ILibraryTable
	{
		bool GetPublishSymbols(_IPreCompileContext precomLocal, string stLibraryIdToLookup);
	}
}
