using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager17 : ILanguageModelManager16, ILanguageModelManager15, ILanguageModelManager14, ILanguageModelManager13, ILanguageModelManager12, ILanguageModelManager11, ILanguageModelManager10, ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		void OnSaveProjectAs(string stOldProjectPath, string stNewProjectPath);

		bool IsExcludedFromBuild(int projectHandle, Guid objectGuid);

		bool IsExcludedFromBuild(int projectHandle, Guid objectGuid, out bool inherited);

		IMessage[] CheckInterfaceLibraryCompatibility(IPreCompileContext pccOlderLibrary, IPreCompileContext pccNewerLibrary);
	}
}
