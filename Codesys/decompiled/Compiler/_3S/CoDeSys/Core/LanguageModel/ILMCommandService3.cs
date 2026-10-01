using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCommandService3 : ILMCommandService2, ILMCommandService
	{
		bool IsAsyncUpdateDownloadInfoInProgress(Guid guidApplication);

		void WaitForAsyncUpdateDownloadInfoCompleted(Guid guidApplication);
	}
}
