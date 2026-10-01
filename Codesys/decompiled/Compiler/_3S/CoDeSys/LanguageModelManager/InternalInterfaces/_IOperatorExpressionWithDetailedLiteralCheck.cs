using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IOperatorExpressionWithDetailedLiteralCheck
	{
		ILiteralValue Literal(IScope scope, bool bAllocatedOK, out bool bOverflow);
	}
}
