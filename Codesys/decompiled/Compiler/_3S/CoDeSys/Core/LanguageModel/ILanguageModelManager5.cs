using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager5 : ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		event EventHandler AfterClearAll;

		void SavePreCompileContextToArchive(IPreCompileContext2 precom, IArchiveWriter writer);

		IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader reader, string stLibraryId);

		void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid, bool bShowPrecompileErrors);

		void ClearAll(bool bAtProjectClose);
	}
}
