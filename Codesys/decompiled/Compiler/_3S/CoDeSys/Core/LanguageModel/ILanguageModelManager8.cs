using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager8 : ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		ISignature2 GetBaseSignature(ISignature2 sign, Guid guidApplication);

		ISignature2[] GetInterfaceSignatures(ISignature2 sign, Guid guidApplication);

		IVariable2[] GetAllVariables(ISignature2 sign, Guid guidApplication);

		ISignature2[] GetAllMethods(ISignature2 sign, Guid guidApplication);

		ISignature2[] GetAllInterfaces(ISignature2 sign, Guid guidApplication);

		bool GenerateCodeForSystemApp(Guid guidApplication, string stLibraryId);

		void SavePreCompileContextToArchive(IPreCompileContext2 precom, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, Profile profile, IArchiveReporter reporter);

		IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader2 reader, string stLibraryId, ISharedDataStorage sharedDataStorage);
	}
}
