using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationSetForInstrumentation
	{
		ILMCompiledParseTreeService CompiledParseTreeService { get; set; }

		ISequenceStatement3 CreateParseTreeOfPOUForInstrumentation(ICompiledPOU cpou);
	}
}
