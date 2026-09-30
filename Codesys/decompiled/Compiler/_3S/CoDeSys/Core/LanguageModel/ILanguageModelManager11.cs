using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelManager11 : ILanguageModelManager10, ILanguageModelManager9, ILanguageModelManager8, ILanguageModelManager7, ILanguageModelManager6, ILanguageModelManager5, ILanguageModelManager4, ILanguageModelManager3, ILanguageModelManager2, ILanguageModelManager
	{
		bool DelayedLoaderWorking { get; }

		event SetLibraryPreCompileContextCompletionPostProcessEventHandler SetLibraryPreCompileContextCompletionPostProcess;

		void CreateBootDuplicate(Guid guidApplication);

		void UpdateDownloadContext(Guid guidApplication, bool bCreateBootDuplicate);

		bool CheckAndLoadBootInfo(Guid guidApplication, Guid guidCode, Guid guidData);
	}
}
