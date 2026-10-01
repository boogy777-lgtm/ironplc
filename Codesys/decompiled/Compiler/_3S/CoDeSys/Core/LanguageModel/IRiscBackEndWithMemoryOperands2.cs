using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEndWithMemoryOperands2 : IRiscBackEndWithMemoryOperands
	{
		void CodeRegisterMemoryOperatorEx(Operator op, TypeClass tc, ref IRegister RegDest, IExpression opMemory);
	}
}
