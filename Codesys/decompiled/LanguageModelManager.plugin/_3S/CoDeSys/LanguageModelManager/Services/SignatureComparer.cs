using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000241 RID: 577
	internal static class SignatureComparer
	{
		// Token: 0x0600266E RID: 9838 RVA: 0x0005FDB0 File Offset: 0x0005EDB0
		public static bool IsEqualPrecompile(_ISignature2 sign1, _ISignature2 sign2, bool bIgnoreFlags, HashSet<string> hsAttributesToIgnore)
		{
			bool flag = false;
			return SignatureComparer.IsEqualPrecompile(sign1, sign2, bIgnoreFlags, hsAttributesToIgnore, null, null, ref flag);
		}

		// Token: 0x0600266F RID: 9839 RVA: 0x0005FDCC File Offset: 0x0005EDCC
		public static bool IsEqualPrecompile(_ISignature2 sign1, _ISignature2 sign2, bool bIgnoreFlags, HashSet<string> hsAttributesToIgnore, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			if (!SignatureComparer.CompareBaseSignature(sign1, sign2))
			{
				return false;
			}
			if (!SignatureComparer.CompareInterfaceExpressions(sign1, sign2))
			{
				return false;
			}
			if (!SignatureComparer.CompareAttributes(sign1, sign2, hsAttributesToIgnore))
			{
				return false;
			}
			if (sign1.Name != sign2.Name)
			{
				return false;
			}
			if (sign1.POUType != sign2.POUType)
			{
				return false;
			}
			if (!bIgnoreFlags && sign1.Flags != sign2.Flags)
			{
				return false;
			}
			if (sign1.POUType == Operator.Interface)
			{
				return true;
			}
			if (sign1.AllVariables.Count != sign2.AllVariables.Count)
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351900)
			{
				return SignatureComparer.CompareVariablesOfSignatures(sign1, sign2, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged);
			}
			return SignatureComparer.CompareVariablesOfSignaturesBeforeSP19(sign1, sign2, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged);
		}

		// Token: 0x06002670 RID: 9840 RVA: 0x0005FE88 File Offset: 0x0005EE88
		private static bool CompareVariablesOfSignaturesBeforeSP19(_ISignature2 sign1, _ISignature2 sign2, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			IVariable[] all = sign1.All;
			for (int i = 0; i < all.Length; i++)
			{
				_IVariable2 ivariable = all[i] as _IVariable2;
				_IVariable2 ivariable2;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
				{
					ivariable2 = (sign2[i] as _IVariable2);
				}
				else
				{
					ivariable2 = (sign2[ivariable.Name] as _IVariable2);
				}
				if (ivariable2 == null || !ivariable.IsEqual(ivariable2, true, true, false, null, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002671 RID: 9841 RVA: 0x0005FEFC File Offset: 0x0005EEFC
		private static bool CompareAttributes(_ISignature2 sign1, _ISignature2 sign2, HashSet<string> hsAttributesToIgnore)
		{
			foreach (string text in sign1.Attributes)
			{
				if (!hsAttributesToIgnore.Contains(text))
				{
					if (!sign2.HasAttribute(text))
					{
						return false;
					}
					string attributeValue = sign1.GetAttributeValue(text);
					string attributeValue2 = sign2.GetAttributeValue(text);
					if (attributeValue != attributeValue2)
					{
						return false;
					}
				}
			}
			foreach (string text2 in sign2.Attributes)
			{
				if (!hsAttributesToIgnore.Contains(text2) && !text2.StartsWith("ieccodeconversion_") && !sign1.HasAttribute(text2))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002672 RID: 9842 RVA: 0x0005FF90 File Offset: 0x0005EF90
		private static bool CompareBaseSignature(_ISignature2 sign1, _ISignature2 sign2)
		{
			return (sign1._BaseSignature != null || sign2._BaseSignature == null) && (sign1._BaseSignature == null || sign2._BaseSignature != null) && (sign1._BaseSignature == null || sign1._BaseSignature.IsEqual(sign2._BaseSignature));
		}

		// Token: 0x06002673 RID: 9843 RVA: 0x0005FFE0 File Offset: 0x0005EFE0
		private static bool CompareInterfaceExpressions(_ISignature2 sign1, _ISignature2 sign2)
		{
			if (sign1.InterfaceExpressions == null && sign2.InterfaceExpressions != null)
			{
				return false;
			}
			if (sign1.InterfaceExpressions != null && sign2.InterfaceExpressions == null)
			{
				return false;
			}
			if (sign1.InterfaceExpressions != null && sign2.InterfaceExpressions != null)
			{
				if (sign1.InterfaceExpressions.Length != sign2.InterfaceExpressions.Length)
				{
					return false;
				}
				for (int i = 0; i < sign1.InterfaceExpressions.Length; i++)
				{
					_IExpression iexpression = sign1.InterfaceExpressions[i] as _IExpression;
					_IExpression expression = sign2.InterfaceExpressions[i] as _IExpression;
					if (!iexpression.IsEqual(expression))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06002674 RID: 9844 RVA: 0x00060070 File Offset: 0x0005F070
		private static bool CompareVariablesOfSignatures(_ISignature2 sign1, _ISignature2 sign2, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			IList<_IVariable> allVariables = sign1.AllVariables;
			IList<_IVariable> allVariables2 = sign2.AllVariables;
			for (int i = 0; i < allVariables.Count; i++)
			{
				_IVariable2 ivariable = allVariables[i] as _IVariable2;
				if (ivariable == null)
				{
					return false;
				}
				_IVariable varRight = allVariables2[i];
				if (!ivariable.IsEqual(varRight, true, true, false, null, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002675 RID: 9845 RVA: 0x000600CC File Offset: 0x0005F0CC
		public static bool IsCompatible(_ISignature sign1, _ISignature sign2)
		{
			if (sign1._BaseSignature == null && sign2._BaseSignature != null)
			{
				return false;
			}
			if (sign1._BaseSignature != null && sign1._BaseSignature == null)
			{
				return false;
			}
			if (sign1._BaseSignature != null && !sign1._BaseSignature.IsEqual(sign2._BaseSignature))
			{
				return false;
			}
			if (sign1.InterfaceExpressions != null && sign2.InterfaceExpressions == null)
			{
				return false;
			}
			if (sign1.InterfaceExpressions != null && sign2.InterfaceExpressions != null)
			{
				if (sign1.InterfaceExpressions.Length > sign2.InterfaceExpressions.Length)
				{
					return false;
				}
				foreach (IExpression expression in sign1.InterfaceExpressions)
				{
					bool flag = false;
					foreach (IExpression expression2 in sign2.InterfaceExpressions)
					{
						if ((expression as _IExpression).IsEqual(expression2 as _IExpression))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			if (sign1.Name != sign2.Name)
			{
				return false;
			}
			if (sign1.POUType != sign2.POUType)
			{
				return false;
			}
			IVariable[] array = sign1.AllInputs;
			IVariable[] array2 = sign2.AllInputs;
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int k = 0; k < array.Length; k++)
			{
				IVariable variable = array[k] as _IVariable;
				_IVariable varRight = array2[k] as _IVariable;
				if (!variable.IsEqual(varRight, false))
				{
					return false;
				}
			}
			array = sign1.AllOutputs;
			array2 = sign2.AllOutputs;
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int l = 0; l < array.Length; l++)
			{
				IVariable variable2 = array[l] as _IVariable;
				_IVariable varRight2 = array2[l] as _IVariable;
				if (!variable2.IsEqual(varRight2, false))
				{
					return false;
				}
			}
			if (sign1.POUType == Operator.FunctionBlock || sign1.POUType == Operator.Type)
			{
				array = sign1.All;
				array2 = sign2.All;
				if (array.Length != array2.Length)
				{
					return false;
				}
				for (int m = 0; m < array.Length; m++)
				{
					IVariable variable3 = array[m] as _IVariable;
					_IVariable varRight3 = array2[m] as _IVariable;
					if (!variable3.IsEqual(varRight3, false))
					{
						return false;
					}
				}
			}
			else if (sign1.POUType == Operator.Program || sign1.POUType == Operator.VarGlobal)
			{
				array = sign1.All;
				foreach (IVariable variable4 in array)
				{
					_IVariable ivariable = sign2[variable4.Name] as _IVariable;
					if (ivariable == null || !variable4.IsEqual(ivariable, false))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
