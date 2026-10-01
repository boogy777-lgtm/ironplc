using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IConstantEvaluator2 : IConstantEvaluator
	{
		ILiteralValue Evaluate(string stExpression, out ISignature owningSignature);

		ILiteralValue Evaluate(IExpression expr, out ISignature owningSignature);
	}
}
