using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd3 : IRiscBackEnd2, IRiscBackEnd
	{
		IRegister GetGlobalDataPointer(TypeClass tc, int nAddress, int iArea, out int nOffset);
	}
}
