using System;
using System.Linq;
using \u0008;
using \u0016;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000B1 RID: 177
	internal sealed class DateOperationConstantFolder : \u0016.\u0002, global::\u0008.\u0004
	{
		// Token: 0x06000E25 RID: 3621 RVA: 0x00025D2C File Offset: 0x00023F2C
		public ILiteralValue \u0001(_IOperatorExpression \u0002, ILiteralValue[] \u0003, out EConstantFoldingResult \u0004)
		{
			\u0004 = EConstantFoldingResult.None;
			try
			{
				TypeClass[] u = \u0002._OperandsList.Select(new Func<_IExpression, TypeClass>(DateOperationConstantFolder.<>c.<>9.\u0001)).ToArray<TypeClass>();
				if (!this.\u0001(\u0003, u))
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

		// Token: 0x06000E26 RID: 3622 RVA: 0x00025DD8 File Offset: 0x00023FD8
		private bool \u0001(ILiteralValue[] \u0002, TypeClass[] \u0003)
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
					if (TypeClass.Time == \u0003[i])
					{
						base.InputValues[i] /= 1000L;
					}
				}
				return true;
			}
		}
	}
}
