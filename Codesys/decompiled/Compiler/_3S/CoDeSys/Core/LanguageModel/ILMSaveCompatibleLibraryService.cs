using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMSaveCompatibleLibraryService
	{
		void SaveVersionedPrecompileSetToArchive(ILMPreCompileSet precom, Version v, IArchiveAuxiliaryWriter auxiliaryWriter, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, IArchiveReporter reporter);
	}
}
