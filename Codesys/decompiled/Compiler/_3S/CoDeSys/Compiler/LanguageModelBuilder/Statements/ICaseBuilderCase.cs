using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements
{
	[ReleasedInterface]
	public interface ICaseBuilderCase : ICaseBuilderOptionalElse, ILmbExprementBuilder<ICaseStatement>, ICaseBuilderElse
	{
		ICaseBuilderCase Case(ICase caseElement);

		ICaseBuilderCase Case(ICaseLabelStatement label, ISequenceStatement2 seq);
	}
}
