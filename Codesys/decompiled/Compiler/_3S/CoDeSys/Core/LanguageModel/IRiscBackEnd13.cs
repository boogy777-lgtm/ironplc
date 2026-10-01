using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd13 : IRiscBackEnd12, IRiscBackEnd11, IRiscBackEnd10, IRiscBackEnd9, IRiscBackEnd8, IRiscBackEnd7, IRiscBackEnd6, IRiscBackEnd5, IRiscBackEnd4, IRiscBackEnd3, IRiscBackEnd2, IRiscBackEnd
	{
		IRegister GenerateExternalFunctionCall(ICallExpression call, ISignature signToCall, IExpression expCallTarget, params IRegister[] regsLocked);
	}
}
