using System;
using \u0008;
using \u0016;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0080
{
	// Token: 0x020000B3 RID: 179
	internal sealed class \u0002 : \u0016.\u0002, global::\u0008.\u0004
	{
		// Token: 0x06000E2B RID: 3627 RVA: 0x00025E84 File Offset: 0x00024084
		public ILiteralValue \u0001(_IOperatorExpression \u0002, ILiteralValue[] \u0003, out EConstantFoldingResult \u0004)
		{
			\u0004 = EConstantFoldingResult.None;
			try
			{
				if (!this.\u0001(\u0003))
				{
					return null;
				}
				\u0002.AcceptOperatorVisitor(this);
			}
			catch (OverflowException)
			{
				\u0004 |= EConstantFoldingResult.Overflow;
				return null;
			}
			catch
			{
				return null;
			}
			if (base.Result != null)
			{
				return \u0019.\u0003.\u0001(base.Result.Value);
			}
			return null;
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x00025F00 File Offset: 0x00024100
		private bool \u0001(ILiteralValue[] \u0002)
		{
			base.InputValues = new long[\u0002.Length];
			checked
			{
				for (int i = 0; i < \u0002.Length; i++)
				{
					ILiteralValue literalValue = \u0002[i];
					if (literalValue.KindOf == KindOfLiteral.SignedInteger)
					{
						base.InputValues[i] = literalValue.SignedLong;
					}
					else
					{
						if (literalValue.KindOf != KindOfLiteral.UnsignedInteger)
						{
							return false;
						}
						base.InputValues[i] = (long)literalValue.UnsignedLong;
					}
				}
				return true;
			}
		}
	}
}
