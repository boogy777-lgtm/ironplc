using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILibraryTable
	{
		IList<_ICompilerMessage> Messages { get; }

		IList<string> AllReferencedLibraries();

		bool LibraryIsUnique(string stLibraryPath);

		string GetLibraryOfPlaceholder(string stPlaceholder);

		string VersionFreeLibraryPath(string stLibraryPath);

		int GetIdOfLibraryReference(string stLibraryId, string stNamespace);

		string GetLibraryById(int nId);

		IList<IPreCompileContext> GetLocalVisibleILibraries();

		IEnumerable<IPreCompileContext> GetVisibleILibraries(string stLibraryId);

		IList<IPreCompileContext> GetVisibleILibraries(_IPreCompileContext precom);

		IList<_IPreCompileContext> GetVisibleLibraries(_IPreCompileContext precom);

		ICaseInsensitiveDictionary<string> GetNamespacesOfLibrary(_IPreCompileContext precomLocal);

		bool? GetQualifiedOnlyRecursive(_IPreCompileContext precomLocal, string stLibraryIdToLookup);

		string GetLocalLibraryNamespaceRecursiveExt(_IPreCompileContext precomLocal, string stLibraryToFind);

		string GetLocalLibraryNamespaceRecursive(_IPreCompileContext precomLocal, string stLibraryToFind);

		string GetLocalLibraryNamespaceRecursive(_IPreCompileContext precomToFind, _IPreCompileContext precomToSearchIn, Dictionary<_IPreCompileContext, _IPreCompileContext> recursionTest);

		string GetNamespaceOfLocalLibrary(string stLibraryIdToLookup);

		bool GetQualifiedOnly(_IPreCompileContext precomLocal, string stLibraryIdToLookup);

		IEnumerable Get32BitOnly();

		string GetNamespaceOfLibrary(_IPreCompileContext precomLocal, string stLibraryIdToLookup);

		_IPreCompileContext GetLibraryContextByNamespace(string stNamespace);

		_IPreCompileContext GetLibraryContextByNamespace(string stNamespace, _IPreCompileContext precomLocal);

		string GetLibraryIdByNamespace(string stNamespace);

		string GetLibraryIdByNamespace(string stNamespace, _IPreCompileContext precomLocal);

		uint CalculateChecksum(_IPreCompileContext precom);
	}
}
