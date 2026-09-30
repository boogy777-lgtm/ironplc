using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface IIfThenBuilder
	{
		IOptionalIfElseOrElseIfsBuilder Then(ISequenceStatement2 controlled);
	}
}
