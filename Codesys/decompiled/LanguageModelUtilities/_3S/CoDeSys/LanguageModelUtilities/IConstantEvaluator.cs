using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IConstantEvaluator
	{
		ILiteralValue Evaluate(string stExpression);

		ILiteralValue Evaluate(IExpression expr);
	}
}
