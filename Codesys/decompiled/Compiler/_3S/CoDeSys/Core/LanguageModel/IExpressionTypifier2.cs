using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpressionTypifier2 : IExpressionTypifier
	{
		void TypifyExpression(IExpression expression);
	}
}
