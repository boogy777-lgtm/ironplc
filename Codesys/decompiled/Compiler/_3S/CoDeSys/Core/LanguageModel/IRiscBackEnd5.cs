using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd5 : IRiscBackEnd4, IRiscBackEnd3, IRiscBackEnd2, IRiscBackEnd
	{
		int StringConstantAlignment { get; }

		void CodeLoadStringByteArray(string st, byte[] bytes, TypeClass tc, IRegister RegDest);
	}
}
