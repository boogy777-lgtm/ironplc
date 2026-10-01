using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCommandService2 : ILMCommandService
	{
		void SavePreCompileSetToArchive(ILMPreCompileSet precom, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, Profile profile, IArchiveReporter reporter, PreCompileSetArchiveStorageFormat archiveStorageFormat);

		void SaveParseTreeToArchive(ILMPreCompileSet precom, ICompiledPOU cpou, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage);
	}
}
