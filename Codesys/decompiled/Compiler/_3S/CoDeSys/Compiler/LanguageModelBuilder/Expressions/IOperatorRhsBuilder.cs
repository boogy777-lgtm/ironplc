using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions
{
	[ReleasedInterface]
	public interface IOperatorRhsBuilder
	{
		ILmbExprementBuilder<IOperatorExpression> Rhs(IExpression rhs);
	}
}
