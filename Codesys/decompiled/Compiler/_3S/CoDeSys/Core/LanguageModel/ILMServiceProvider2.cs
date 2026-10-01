using _3S.CoDeSys.Compiler.LanguageModelServices;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMServiceProvider2 : ILMServiceProvider
	{
		ILMPouSetPersistenceService LMPouSetPersistenceService { get; }

		ILMCachingService CachingService { get; }
	}
}
