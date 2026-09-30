using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd4 : IRiscBackEnd3, IRiscBackEnd2, IRiscBackEnd
	{
		bool GetProperty(CodegeneratorProperties cgpProperty);
	}
}
