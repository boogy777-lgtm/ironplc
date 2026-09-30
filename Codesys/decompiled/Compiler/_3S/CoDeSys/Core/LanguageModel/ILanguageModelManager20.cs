using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager20 : ILanguageModelManager19, ILanguageModelManager18, ILanguageModelManager17, ILanguageModelManager16, ILanguageModelManager15, ILanguageModelManager14, ILanguageModelManager13, ILanguageModelManager12, ILanguageModelManager11, ILanguageModelManager10, ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		bool IsResolvedXType(IType type);

		IMessage[] CheckLibraryCompatibility(IPreCompileContext pccOlderLibrary, IPreCompileContext pccNewerLibrary, bool bInterfaceLibrary);
	}
}
