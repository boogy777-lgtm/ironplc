using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMProviderService2 : ILMProviderService
	{
		void RemoveLanguageModelOfObject(string libraryId, Guid objectGuid);
	}
}
