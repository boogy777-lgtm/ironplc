using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder4 : ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		IHasConstantValueExpression CreateHasConstantValueExpression(IExprementPosition pos, IExpression constant, ILiteralExpression value);
	}
}
