using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAtomicLoadStore64BackEnd
	{
		IRegister CodeLoadAbsoluteAtomic64(TypeClass tc, IRegister RegBase, IRegister RegIndex, int nOffset, int nScale, out bool bLoaded);

		bool CodeStoreAbsoluteAtomic64(TypeClass tc, IRegister RegBase, IRegister RegIndex, int nOffset, int nScale, IRegister RegSrc);
	}
}
