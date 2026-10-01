using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions
{
	[ReleasedInterface]
	public interface ICalleeInputParamAssignsBuilder : ICalleeOutputsAssignsBuilder, ILmbExprementBuilder<ICallExpression>
	{
		ICalleeInputParamAssignsBuilder AddParam(IExpression exp, IExpression expVariable);
	}
}
