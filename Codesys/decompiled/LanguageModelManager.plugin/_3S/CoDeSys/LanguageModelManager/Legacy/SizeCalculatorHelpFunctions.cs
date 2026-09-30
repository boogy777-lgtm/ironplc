using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x0200027C RID: 636
	internal static class SizeCalculatorHelpFunctions
	{
		// Token: 0x06002ABC RID: 10940 RVA: 0x0006ECF4 File Offset: 0x0006DCF4
		internal static int CalculateNumOfInterfaces(ISignature3 sign, IExpression[] expInterfaces, IPrecompileScope4 prescope)
		{
			int num = 0;
			foreach (IExpression expression in expInterfaces)
			{
				bool flag = false;
				sign = SizeCalculatorHelpFunctions.HandleBaseInterfaces(sign, prescope, expression, ref flag);
				if (!flag)
				{
					num++;
					ISignature3 signature = prescope.FindSignatureGlobal(expression) as ISignature3;
					if (signature == null)
					{
						return -1;
					}
					int num2 = SizeCalculatorHelpFunctions.CalculateNumOfInterfaces(sign, signature.InterfaceExpressions, prescope);
					if (num2 == -1)
					{
						return -1;
					}
					num += num2;
				}
			}
			return num;
		}

		// Token: 0x06002ABD RID: 10941 RVA: 0x0006ED60 File Offset: 0x0006DD60
		private static ISignature3 HandleBaseInterfaces(ISignature3 sign, IPrecompileScope4 prescope, IExpression expInterface, ref bool bInterfaceDerived)
		{
			while (sign.BaseExpression != null)
			{
				ISignature3 signature = prescope.FindSignatureGlobal(sign.BaseExpression) as ISignature3;
				if (signature == null)
				{
					break;
				}
				foreach (IExpression expression in signature.InterfaceExpressions)
				{
					if (string.Compare(expInterface.ToString(), expression.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
					{
						bInterfaceDerived = true;
					}
				}
				sign = signature;
			}
			return sign;
		}

		// Token: 0x06002ABE RID: 10942 RVA: 0x0006EDC0 File Offset: 0x0006DDC0
		internal static bool GetConstExpressionValue(IExpression2 expValue, IPrecompileScope4 prescope, IRecursionGuard recursionGuard, out int nValue)
		{
			nValue = -1;
			bool flag;
			ILiteralValue literalValue = ((_IExpression2)expValue).LiteralWithRecursionCheck(prescope, recursionGuard, false, out flag);
			if (literalValue == null || flag)
			{
				return false;
			}
			bool result;
			nValue = literalValue.GetInt(out result);
			return result;
		}
	}
}
