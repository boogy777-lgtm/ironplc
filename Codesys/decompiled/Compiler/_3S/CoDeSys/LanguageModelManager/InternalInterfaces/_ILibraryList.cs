using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILibraryList
	{
		_IPreCompileContext[] AllLibraryContexts { get; }

		_IPreCompileContext this[string stId] { get; set; }

		_IPreCompileContext GetLibraryContext(string stLibraryId);

		_IPreCompileContext GetLibraryContext(string stLibraryId, bool bCreateIfNotExist);

		void AddLibraryReference(Guid guidLibman, string stId);

		string[] GetReferencedLibraries(Guid guidLibMan);

		Guid[] GetLibraryReferences(string stId);

		void RemoveLibraryReferences(Guid libmanGuid);

		void RemoveLibrary(string stId);

		void CheckForUnreferencedLibraries();

		int GetProjectHandle(string stLibraryId);

		IProject GetProjectByLibraryId(string stLibraryId);
	}
}
