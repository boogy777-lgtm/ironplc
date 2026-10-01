using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscFrontEnd
	{
		ICompiledPOU CurrentPOU { get; }

		IExprement CurrentExpression { get; }

		IRegisterManager CreateDefaultRegisterManager();

		IRiscCompiledCode CreateDefaultCompiledCode();

		void CodeStringCopy(IRegister RegSrc, IRegister RegDest, uint uiSize, TypeClass tc);

		void CodeMemcopy(IRegister RegSrc, IRegister RegDest, uint count);

		void CodeStringCompare(Operator oc, TypeClass tc, IRegister Reg1, IRegister Reg2, ref IRegister RegDest);

		void CodeMinMax(Operator op, TypeClass tc, IRegister Reg1, IRegister Reg2);

		void CodeSel(TypeClass tc, IRegister Reg1, IRegister Reg2, IRegister RegDest);

		void CodeLIntegerCompare(Operator op, TypeClass tc, IRegister Reg1, IRegister Reg2, ref IRegister RegDest);
	}
}
