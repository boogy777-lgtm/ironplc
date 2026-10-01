using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager10 : ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		event EventHandler AfterLazyLibraryLoad;

		bool GenerateOnlineChangeCode(Guid guidApplication, bool bKeepCompileInformation, out IOnlineChangeDetails ocd);
	}
}
