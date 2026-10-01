using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEndVectorUnit
	{
		int VectorRegisterSize { get; }

		int VectorAlignment { get; }

		void CodeLoadVec(TypeClass basetc, int dimension, bool fullRegister, IRegister regBase, IRegister regIndex, int nOffset, int nScale, ref IRegister regDest);

		void CodeStoreVec(TypeClass basetc, int dimension, bool fullRegister, IRegister regBase, IRegister regIndex, int nOffset, int nScale, IRegister regSrc);

		void CodeSetComponent(TypeClass basetc, IRegister regDest, int idDest, IRegister regSrc, int idSrc);

		void CodeOperatorVecVec(Operator op, TypeClass basetc, int dimension, IRegister reg1, IRegister reg2, ref IRegister regDest);

		void CodeOperatorScalarVec(Operator op, TypeClass basetc, int dimension, IRegister scalarReg, IRegister vecReg, bool flipOrder, ref IRegister regDest);

		void CodeSetZeroVec(TypeClass basetc, IRegister reg);

		int VectorRegisterElements(TypeClass tc);

		IRegister AllocateRegister(TypeClass basetc, int dimension);

		void CodeOperatorScalarVecMultiplyAdd(TypeClass basetc, int dimension, IRegister accumReg, IRegister scalarReg, IRegister vecReg);

		void CodeOperatorVecVecMultiplyAdd(TypeClass basetc, int dimension, IRegister accumReg, IRegister vecReg1, IRegister vecReg2);
	}
}
