using System;
using System.Collections.Generic;
using \u0015;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000D1 RID: 209
	public class VariableComparer
	{
		// Token: 0x06000EDC RID: 3804 RVA: 0x00028E74 File Offset: 0x00027074
		public static bool InitialValueEquals(_IVariable left, _IVariable right, bool bCompiled)
		{
			IExpression initial = left.Initial;
			IExpression initial2 = right.Initial;
			if (initial == null != (initial2 == null))
			{
				return false;
			}
			if (initial != null && initial2 != null)
			{
				if (!bCompiled && initial is _ILiteralExpression && initial2 is _ILiteralExpression)
				{
					_ILiteralExpression iliteralExpression = (_ILiteralExpression)initial;
					_ILiteralExpression other = (_ILiteralExpression)initial2;
					if (!iliteralExpression.LiteralValueEquals(other))
					{
						return false;
					}
				}
				else if (!left._Initial.IsEqual(right._Initial))
				{
					return false;
				}
			}
			if (left.InputAssignments == null != (right.InputAssignments == null))
			{
				return false;
			}
			if (left.InputAssignments != null)
			{
				if (left.InputAssignments.Length != right.InputAssignments.Length)
				{
					return false;
				}
				for (int i = 0; i < left.InputAssignments.Length; i++)
				{
					if (!(right.InputAssignments[i] as _IExprement).IsEqual(left.InputAssignments[i] as _IExprement))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000EDD RID: 3805 RVA: 0x00028F4C File Offset: 0x0002714C
		public static bool IsEqual(_IVariable varLeft, _IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			VarFlag varFlag = VarFlag.Local | VarFlag.Input | VarFlag.Output | VarFlag.Inout | VarFlag.External | VarFlag.ReplacedConstant | VarFlag.Constant | VarFlag.Enum | VarFlag.Alias | VarFlag.Structure | VarFlag.Retain | VarFlag.Persistent | VarFlag.VarConfig | VarFlag.Global | VarFlag.VarAccess | VarFlag.LocalPersistent;
			if (bCompiled)
			{
				varFlag |= VarFlag.IsCompiled;
			}
			if ((varRight.Flags & varFlag) != (varLeft.Flags & varFlag))
			{
				return false;
			}
			if (!VariableComparer.\u0001(varLeft, varRight))
			{
				return false;
			}
			if (bCompiled)
			{
				if (varRight._Type != null && !((ICompiledType3)varRight._Type.EffectiveType).IsEqual(varLeft._Type.EffectiveType, scope))
				{
					return false;
				}
			}
			else if (!VariableComparer.\u0001(varLeft, varRight, bCompiled, scope, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged))
			{
				return false;
			}
			return !(varRight.Name != varLeft.Name) && VariableComparer.\u0002(varLeft, varRight) && (!bCompareAttributes || VariableComparer.\u0003(varLeft, varRight)) && (!bCompareInitValues || VariableComparer.InitialValueEquals(varLeft, varRight, bCompiled));
		}

		// Token: 0x06000EDE RID: 3806 RVA: 0x00029014 File Offset: 0x00027214
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003)
		{
			return (\u0002._Type != null || \u0003.Type == null) && (\u0002._Type == null || \u0003._Type != null);
		}

		// Token: 0x06000EDF RID: 3807 RVA: 0x0002903C File Offset: 0x0002723C
		private static bool \u0002(_IVariable \u0002, _IVariable \u0003)
		{
			return (\u0003.Address != null || \u0002.Address == null) && (\u0003.Address == null || \u0002.Address != null) && (\u0003.Address == null || \u0003.Address.IsEqual(\u0002.Address));
		}

		// Token: 0x06000EE0 RID: 3808 RVA: 0x0002908C File Offset: 0x0002728C
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, bool \u0004, IScope \u0005, IPrecompileScope \u0006, IPrecompileScope \u0007, ref bool \u0008)
		{
			_IAliasType ialiasType = \u0002.OriginalType as _IAliasType;
			if (ialiasType != null)
			{
				return ialiasType.ToString() == \u0003._Type.ToString();
			}
			if (\u0003._Type != null && ((ICompiledType5)\u0003._Type.EffectiveType) is _IArrayType2 && \u0002._Type.EffectiveType is _IArrayType2 && \u0006 != null && \u0007 != null)
			{
				if (!(((ICompiledType5)\u0003._Type.EffectiveType) as _IArrayType2).IsEqual(\u0002._Type.EffectiveType, \u0005, \u0004, \u0006, \u0007, ref \u0008))
				{
					return false;
				}
			}
			else if (\u0002.GetFlag(VarFlag.Inout) && \u0002._Type.Class == TypeClass.Reference && \u0003._Type != null)
			{
				if (!((ICompiledType5)\u0003._Type.EffectiveType).IsEqualPreCompile(\u0002._Type.DeRefType, \u0005))
				{
					return false;
				}
			}
			else if (\u0003._Type != null && !((ICompiledType5)\u0003._Type.EffectiveType).IsEqualPreCompile(\u0002._Type.EffectiveType, \u0005))
			{
				return false;
			}
			return true;
		}

		// Token: 0x06000EE1 RID: 3809 RVA: 0x000291A0 File Offset: 0x000273A0
		private static bool \u0003(_IVariable \u0002, _IVariable \u0003)
		{
			\u0001 u = new \u0001(IgnoreAttributes.Comment | IgnoreAttributes.DocuComment | IgnoreAttributes.MessageGuid | IgnoreAttributes.VarLenArrayOriginalScope);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			foreach (string text in \u0003.Attributes)
			{
				dictionary2.Add(text, \u0003.GetAttributeValue(text));
			}
			foreach (string text2 in \u0002.Attributes)
			{
				dictionary.Add(text2, \u0002.GetAttributeValue(text2));
			}
			return u.\u0001(dictionary2, dictionary);
		}
	}
}
