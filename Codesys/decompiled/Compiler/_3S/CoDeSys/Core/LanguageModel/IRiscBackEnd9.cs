using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd9 : IRiscBackEnd8, IRiscBackEnd7, IRiscBackEnd6, IRiscBackEnd5, IRiscBackEnd4, IRiscBackEnd3, IRiscBackEnd2, IRiscBackEnd
	{
		object PrepareCall(bool bExternal);

		void FinishCall(bool bExternal, object o);
	}
}
