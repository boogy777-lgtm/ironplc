using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMServiceProvider5 : ILMServiceProvider4, ILMServiceProvider3, ILMServiceProvider2, ILMServiceProvider
	{
		ILMNameManglingService NameManglingService { get; }

		ILMTransitionUserCodeAnalyzerService TransitionUserCodeAnalyzerService { get; }
	}
}
