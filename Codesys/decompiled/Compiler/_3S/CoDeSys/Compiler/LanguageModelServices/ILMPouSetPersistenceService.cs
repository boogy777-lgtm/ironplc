using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelServices
{
	[ReleasedInterface]
	public interface ILMPouSetPersistenceService
	{
		ILMCompiledApplicationSet LoadFromFile(string filePath);
	}
}
