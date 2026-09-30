using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000101 RID: 257
	internal interface IVarReferenceGenerator
	{
		// Token: 0x060012C3 RID: 4803
		void Init(IScope5 scope, ICompileContext comcon);

		// Token: 0x060012C4 RID: 4804
		IAddressInfo GenerateVarAbsolut(IVariableExpression varexp, IExpression expInstancePath, int iArea, int nAddress, int nSize);

		// Token: 0x060012C5 RID: 4805
		IAddressInfo GeneratePropertyCall(_IVariableExpression varExp, IExpression expInstancePath, IAddressInfo adrInfoInstance, int nSize);

		// Token: 0x060012C6 RID: 4806
		IAddressInfo GenerateDirectAddress(IAddressExpression addr, IDataLocation datloc, int nSize);

		// Token: 0x060012C7 RID: 4807
		IAddressInfo GenerateIndexAccess(IIndexAccessExpression indexaccess, int iArea, int nAddress, int nSize);

		// Token: 0x060012C8 RID: 4808
		IAddressInfo GenerateVariableIndexAccess(IIndexAccessExpression indexaccess, IAddressInfo aiWholeArray, int nBaseSize, IAddressInfo[] indexAccesses, IArrayBounds[] arrayBounds, IType t);

		// Token: 0x060012C9 RID: 4809
		IAddressInfo GenerateVarStackRelative(ICompiledPOU cpou, IVariableExpression varexp, IExpression expInstancePath, int nAddress, int nSize);

		// Token: 0x060012CA RID: 4810
		IAddressInfo GenerateVarStackRelativeOffset(IVariableExpression varexp, IExpression expInstancePath, StackRelativeAddressInfo stinfo, int nAddress, int nSize);

		// Token: 0x060012CB RID: 4811
		IAddressInfo GenerateOperatorExpression(_IOperatorExpression opExp, IAddressInfo[] addrsOperands);

		// Token: 0x060012CC RID: 4812
		IAddressInfo GenerateSignedConstant(IExpression constExpr, int literalValue);

		// Token: 0x060012CD RID: 4813
		IAddressInfo GenerateAddress(ulong literalValue);

		// Token: 0x060012CE RID: 4814
		IAddressInfo GenerateAddress(int iArea, int iOffset);

		// Token: 0x060012CF RID: 4815
		void AdaptFunctionCallInfoSourceposition(IAddressInfo addressInfo, _ICallExpression callexp);

		// Token: 0x060012D0 RID: 4816
		IAddressInfo GenerateFunctionCall(_IVariableExpression variable, ISignature sign, _IExpression expInstancePath, IScope5 scope);

		// Token: 0x060012D1 RID: 4817
		IAddressInfo GenerateDeRefAccess(IExpression deref, IAddressInfo varrefelement, int nOffset);

		// Token: 0x060012D2 RID: 4818
		IAddressInfo GenerateCompoAccess(IExpression compo, IAddressInfo varrefelement, int nOffset);

		// Token: 0x060012D3 RID: 4819
		IAddressInfo GenerateBitAccess(IExpression compo, ICompiledType leftType, ISourcePosition varRefPosition, IAddressInfo varrefelement, int nBitOffset, int accessedElementSize);

		// Token: 0x060012D4 RID: 4820
		IAddressInfo GenerateLiteral(IVariableExpression varexp, IExpression expInstancePath, ILiteralValue lv);

		// Token: 0x060012D5 RID: 4821
		void ClearDueToInvalidArrayIndex();

		// Token: 0x060012D6 RID: 4822
		void Remove(IAddressInfo aiInfo);
	}
}
