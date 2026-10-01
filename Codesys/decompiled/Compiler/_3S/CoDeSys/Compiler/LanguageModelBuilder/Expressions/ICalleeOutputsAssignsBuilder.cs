using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions
{
	[ReleasedInterface]
	public interface ICalleeOutputsAssignsBuilder : ILmbExprementBuilder<ICallExpression>
	{
		ICalleeOutputsAssignsBuilder AddOutput(IExpression exp, IExpression expVariable);
	}
}
