using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileCheckerService
	{
		bool PrecompileChecksDone { get; set; }

		event CompileEventHandler AfterPrecompileChecksDone;

		void FinishPrecompileChecks();
	}
}
