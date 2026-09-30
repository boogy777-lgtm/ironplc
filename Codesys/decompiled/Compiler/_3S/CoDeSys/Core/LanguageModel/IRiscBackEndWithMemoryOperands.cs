using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEndWithMemoryOperands
	{
		bool IsMemoryOperand(IExpression exprOperand, TypeClass tc, Operator op);

		[Obsolete("Use CodeRegisterMemoryOperatorEx() instead.")]
		void CodeRegisterMemoryOperator(Operator op, TypeClass tc, IRegister RegDest, IExpression opMemory);
	}
}
