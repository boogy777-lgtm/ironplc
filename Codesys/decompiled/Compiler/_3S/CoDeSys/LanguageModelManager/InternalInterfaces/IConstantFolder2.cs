using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IConstantFolder2 : IConstantFolder
	{
		ILiteralValue LiteralWithRecursionCheck(_IOperatorExpression opexp, IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError);

		ILiteralValue LiteralWithRecursionCheck(_IOperatorExpression opexp, IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError);

		ILiteralValue LiteralUnchecked(_IOperatorExpression opexp, IScope scope);
	}
}
