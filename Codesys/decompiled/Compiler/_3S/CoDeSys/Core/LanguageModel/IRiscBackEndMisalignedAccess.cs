using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEndMisalignedAccess
	{
		IRegister GetGlobalDataPointer(TypeClass tc, int nAddress, int iArea, out int nOffset, out bool bMisaligned);

		void CodeLoadMisaligned(TypeClass tc, IRegister RegBase, IRegister RegIndex, int nOffset, IRegister RegDest, int nScale);

		void CodeStoreMisaligned(TypeClass tc, IRegister RegBase, IRegister RegIndex, int nOffset, IRegister RegSrc, int nScale);
	}
}
