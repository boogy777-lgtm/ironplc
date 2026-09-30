using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledParseTreeService
	{
		ISequenceStatement3 CreateParseTreeOfPOUForInstrumentation(ICompiledPOU cpou);

		void TypeCheckTemporaryParseTree(ICompiledPOU cpou, ISequenceStatement3 sequenceStatement);
	}
}
