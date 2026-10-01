using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMTransitionUserCodeAnalyzerService
	{
		ITransitionUserCodeAnalyzationResult Analyze(string stCode, string stTransName);
	}
}
