using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationDebugging2 : ILMCompiledApplicationDebugging
	{
		void RemoveWatchVariable(string stName);
	}
}
