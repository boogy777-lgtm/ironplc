using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder6 : ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		IHasConstantValueExpression2 CreateHasConstantValueExpression(IExprementPosition pos, IExpression constant, ILiteralExpression value, Operator comparison);

		void SetExprementPosition(IExprementPosition expPos, IExprement exp);
	}
}
