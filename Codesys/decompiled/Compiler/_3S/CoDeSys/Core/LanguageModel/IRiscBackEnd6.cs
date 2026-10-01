using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd6 : IRiscBackEnd5, IRiscBackEnd4, IRiscBackEnd3, IRiscBackEnd2, IRiscBackEnd
	{
		IRegister GenerateExternalFunctionCall(ICallExpression call, ISignature signToCall, IExpression expCallTarget);

		void CodeExtendRegister(TypeClass tc, IRegister Reg);
	}
}
