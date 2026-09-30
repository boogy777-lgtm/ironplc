using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd10 : IRiscBackEnd9, IRiscBackEnd8, IRiscBackEnd7, IRiscBackEnd6, IRiscBackEnd5, IRiscBackEnd4, IRiscBackEnd3, IRiscBackEnd2, IRiscBackEnd
	{
		bool NeedsAtomicAccessCall(TypeClass tc, bool bWrite);
	}
}
