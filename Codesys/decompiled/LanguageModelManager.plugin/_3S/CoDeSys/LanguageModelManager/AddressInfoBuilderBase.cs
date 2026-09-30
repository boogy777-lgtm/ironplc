using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E3 RID: 227
	internal abstract class AddressInfoBuilderBase
	{
		// Token: 0x0600111B RID: 4379 RVA: 0x0003192F File Offset: 0x0003092F
		protected virtual IAddressInfo DoGenerateVariableIndexAccess(IIndexAccessExpression indexaccess, IAddressInfo aiWholeArray, int nBaseSize, IAddressInfo[] indexAccesses, IArrayBounds[] arrayBounds, IType t)
		{
			if (aiWholeArray == null || indexaccess == null || arrayBounds == null || indexAccesses.Length != arrayBounds.Length)
			{
				return null;
			}
			return new VariableArrayAccessAddressInfo(aiWholeArray, nBaseSize, indexAccesses, arrayBounds, t);
		}

		// Token: 0x0600111C RID: 4380 RVA: 0x00031954 File Offset: 0x00030954
		protected virtual IAddressInfo DoGenerateOperatorExpression(_IOperatorExpression opExp, IAddressInfo[] addrsOperands)
		{
			if (opExp == null || addrsOperands == null || addrsOperands.Length == 0)
			{
				return null;
			}
			return new OperatorAddressInfo(addrsOperands, opExp.Code, opExp.Type);
		}

		// Token: 0x0600111D RID: 4381 RVA: 0x00031974 File Offset: 0x00030974
		protected virtual IAddressInfo DoGenerateSignedConstant(IExpression constExpr, int iValue)
		{
			if (constExpr == null)
			{
				return null;
			}
			return new SignedConstantAddressInfo((long)iValue);
		}

		// Token: 0x0600111E RID: 4382 RVA: 0x00031982 File Offset: 0x00030982
		protected virtual IAddressInfo DoGenerateAddress(ulong ulValue)
		{
			return new AddressAddressInfo(ulValue);
		}

		// Token: 0x0600111F RID: 4383 RVA: 0x0003198A File Offset: 0x0003098A
		protected virtual IAddressInfo DoGenerateAddress(int iArea, int iOffset)
		{
			return new AddressAddressInfo(iArea, iOffset);
		}
	}
}
