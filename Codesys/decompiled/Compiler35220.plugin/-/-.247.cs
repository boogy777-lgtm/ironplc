using System;
using System.Collections.Generic;
using \u000E;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001A
{
	// Token: 0x020002AC RID: 684
	internal static class \u0011
	{
		// Token: 0x06002A99 RID: 10905 RVA: 0x00095248 File Offset: 0x00093448
		public static _IExpression \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			IVariable variable = operandsList[0].GetVariable(\u0003._Scope);
			string stInput;
			if (variable != null && !variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
			{
				bool flag;
				int num = (int)((_ILiteralValue)\u0002._OperandsList[1].Literal(\u0003._Scope)).GetAnyLong(out flag);
				IArrayDimension arrayDimension = ((IArrayType)operandsList[0].Type.DeRefType).Dimensions[num - 1];
				stInput = \u0011.\u0001(((Operator.LowerBound == \u0002.Code) ? arrayDimension.LowerBorder : arrayDimension.UpperBorder).Literal(\u0003._Scope));
			}
			else if (\u0002.Code == Operator.LowerBound)
			{
				stInput = string.Format("{0}__Array__Info[{1}].diLower", \u0002._OperandsList[0], \u0002._OperandsList[1]);
			}
			else
			{
				stInput = string.Format("{0}__Array__Info[{1}].diUpper", \u0002._OperandsList[0], \u0002._OperandsList[1]);
			}
			return \u0003.Generator.GenerateExpression(stInput, \u0003._Scope, \u0003.CompiledPOU);
		}

		// Token: 0x06002A9A RID: 10906 RVA: 0x0009536C File Offset: 0x0009356C
		private static string \u0001(ILiteralValue \u0002)
		{
			if (\u0002.KindOf == KindOfLiteral.SignedInteger)
			{
				return \u0002.SignedLong.ToString();
			}
			return \u0002.UnsignedLong.ToString();
		}
	}
}
