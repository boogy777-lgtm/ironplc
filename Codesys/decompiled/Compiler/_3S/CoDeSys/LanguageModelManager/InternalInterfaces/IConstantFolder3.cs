using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IConstantFolder3 : IConstantFolder2, IConstantFolder
	{
		ILiteralValue GetLiteralValue(_IExpression exp, ICommonScope scope);

		ILiteralValue GetLiteralValue(_IExpression exp, IPrecompileScope scope, out bool bRecursionError);

		ILiteralValue GetLiteralValue(_IExpression exp, IScope scope, out bool bRecursionError);

		ILiteralValue GetLiteralValue(_IExpression exp, IPrecompileScope scope, IRecursionGuard recursionGuard, out bool bRecursionError);

		ILiteralValue GetLiteralValue(_IExpression exp, IScope scope, IRecursionGuard recursionGuard, out bool bRecursionError);

		ILiteralValue LiteralWithRecursionCheck(_IOperatorExpression opexp, IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out EConstantFoldingResult eResult);

		ILiteralValue GetLiteral(_IOperatorExpression opexp, ILiteralValue[] litvalOps, bool bPrecompile, out EConstantFoldingResult eResult);
	}
}
