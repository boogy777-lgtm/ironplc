using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001C
{
	// Token: 0x02000130 RID: 304
	internal static class \u0007
	{
		// Token: 0x060015AF RID: 5551 RVA: 0x0003F21C File Offset: 0x0003D41C
		internal static int \u0001(ISignature3 \u0002, IExpression[] \u0003, IPrecompileScope4 \u0004)
		{
			int num = 0;
			foreach (IExpression expression in \u0003)
			{
				bool flag = false;
				\u0002 = \u0007.\u0001(\u0002, \u0004, expression, ref flag);
				if (!flag)
				{
					num++;
					ISignature3 signature = \u0004.FindSignatureGlobal(expression) as ISignature3;
					if (signature == null)
					{
						return -1;
					}
					int num2 = \u0007.\u0001(\u0002, signature.InterfaceExpressions, \u0004);
					if (num2 == -1)
					{
						return -1;
					}
					num += num2;
				}
			}
			return num;
		}

		// Token: 0x060015B0 RID: 5552 RVA: 0x0003F288 File Offset: 0x0003D488
		private static ISignature3 \u0001(ISignature3 \u0002, IPrecompileScope4 \u0003, IExpression \u0004, ref bool \u0005)
		{
			while (\u0002.BaseExpression != null)
			{
				ISignature3 signature = \u0003.FindSignatureGlobal(\u0002.BaseExpression) as ISignature3;
				if (signature == null)
				{
					break;
				}
				foreach (IExpression expression in signature.InterfaceExpressions)
				{
					if (string.Compare(\u0004.ToString(), expression.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
					{
						\u0005 = true;
					}
				}
				\u0002 = signature;
			}
			return \u0002;
		}

		// Token: 0x060015B1 RID: 5553 RVA: 0x0003F2E8 File Offset: 0x0003D4E8
		internal static bool \u0001(IExpression2 \u0002, IPrecompileScope4 \u0003, IRecursionGuard \u0004, out int \u0005)
		{
			\u0005 = -1;
			bool flag;
			ILiteralValue literalValue = ((_IExpression2)\u0002).LiteralWithRecursionCheck(\u0003, \u0004, false, out flag);
			if (literalValue == null || flag)
			{
				return false;
			}
			bool result;
			\u0005 = literalValue.GetInt(out result);
			return result;
		}
	}
}
