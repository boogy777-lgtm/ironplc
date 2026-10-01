using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager15 : ILanguageModelManager14, ILanguageModelManager13, ILanguageModelManager12, ILanguageModelManager11, ILanguageModelManager10, ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		event CompileEventHandler AfterGenerateCode;

		bool CompileAndLocate(Guid guidApplication);

		bool CheckAllApplicationObjects(Guid guidApplication);

		IApplicationContent BuildApplicationContentFromUpload(byte[] bytes);

		[Obsolete("Use IsHiddenVariable(ISignature6, IVariable, GUIHidingFlags) instead")]
		bool IsHiddenVariable(IVariable variable, GUIHidingFlags flagsToConsider);

		bool IsHiddenSignature(ISignature signature, GUIHidingFlags flagsToConsider);

		bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation, out IMessage[] errors, out IMessage[] warnings);
	}
}
