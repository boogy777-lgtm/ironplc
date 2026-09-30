using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEnd2 : IRiscBackEnd
	{
		void CodeMemoryBitAccess(IRegister RegSrc, IRegister RegDest, Access access, int nBitNr, TypeClass tc);
	}
}
