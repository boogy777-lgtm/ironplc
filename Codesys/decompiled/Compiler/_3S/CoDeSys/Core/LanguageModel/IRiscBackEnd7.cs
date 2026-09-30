using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd7 : IRiscBackEnd6, IRiscBackEnd5, IRiscBackEnd4, IRiscBackEnd3, IRiscBackEnd2, IRiscBackEnd
	{
		int GetExternalInputSize(int iIndexInput, IVariable varInput);
	}
}
