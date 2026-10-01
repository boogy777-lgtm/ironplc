using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscFrontEnd3 : IRiscFrontEnd2, IRiscFrontEnd
	{
		void CodeInlineStringCompare(Operator oc, TypeClass tc, IRegister Reg1, IRegister Reg2, ref IRegister RegDest);

		void CodeInlineMemcopy(IRegister RegSrc, IRegister RegDest, uint uiCount);

		void CodeInlineStringCopy(IRegister RegSrc, IRegister RegDest, uint uiSize, TypeClass tc);
	}
}
