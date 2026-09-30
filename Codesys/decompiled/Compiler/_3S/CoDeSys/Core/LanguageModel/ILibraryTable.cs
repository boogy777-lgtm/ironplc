using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryTable
	{
		IPreCompileContext9 GetLibraryContextByNamespace(string stNamespace, IPreCompileContext9 precomlocal);

		IList<IPreCompileContext9> GetVisibleLibraries(IPreCompileContext9 precomlocal);

		string GetNamespaceOfLibrary(string stLibraryId, IPreCompileContext9 precomlocal);
	}
}
