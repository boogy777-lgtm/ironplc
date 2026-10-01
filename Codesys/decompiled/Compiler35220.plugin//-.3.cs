using System;
using System.Runtime.CompilerServices;
using \u0008;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0082
{
	// Token: 0x020000B0 RID: 176
	internal sealed class \u0002 : \u0082.\u0003, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor, global::\u0008.\u0004
	{
		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x06000E17 RID: 3607 RVA: 0x000255C0 File Offset: 0x000237C0
		// (set) Token: 0x06000E18 RID: 3608 RVA: 0x000255C8 File Offset: 0x000237C8
		private long[] InputValues { get; set; }

		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x06000E19 RID: 3609 RVA: 0x000255D4 File Offset: 0x000237D4
		// (set) Token: 0x06000E1A RID: 3610 RVA: 0x000255DC File Offset: 0x000237DC
		private long? Result { get; set; }

		// Token: 0x06000E1B RID: 3611 RVA: 0x000255E8 File Offset: 0x000237E8
		public ILiteralValue \u0001(_IOperatorExpression \u0002, ILiteralValue[] \u0003, out EConstantFoldingResult \u0004)
		{
			\u0004 = EConstantFoldingResult.None;
			try
			{
				ILiteralValue result;
				if (this.\u0001(\u0002, \u0003, out result))
				{
					return result;
				}
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
			if (this.Result != null)
			{
				return \u0019.\u0003.\u0001(this.Result.Value);
			}
			return null;
		}

		// Token: 0x06000E1C RID: 3612 RVA: 0x00025674 File Offset: 0x00023874
		private bool \u0001(_IOperatorExpression \u0002, ILiteralValue[] \u0003, out ILiteralValue \u0004)
		{
			bool flag = \u0002.Code == Operator.Trunc || \u0002.Code == Operator.TruncInt;
			bool flag2 = \u0003.Length == 1 && \u0003[0].KindOf == KindOfLiteral.Float;
			if (flag && flag2)
			{
				double @float = \u0003[0].Float;
				long u;
				if (\u0002.Code == Operator.Trunc)
				{
					u = checked((long)@float);
				}
				else
				{
					u = (long)(checked((short)@float));
				}
				\u0004 = \u0019.\u0003.\u0001(u);
				return true;
			}
			\u0004 = null;
			return false;
		}

		// Token: 0x06000E1D RID: 3613 RVA: 0x000256E0 File Offset: 0x000238E0
		private bool \u0001(ILiteralValue[] \u0002)
		{
			this.InputValues = new long[\u0002.Length];
			checked
			{
				for (int i = 0; i < \u0002.Length; i++)
				{
					ILiteralValue literalValue = \u0002[i];
					if (literalValue.KindOf == KindOfLiteral.SignedInteger)
					{
						this.InputValues[i] = literalValue.SignedLong;
					}
					else
					{
						if (literalValue.KindOf != KindOfLiteral.UnsignedInteger)
						{
							return false;
						}
						this.InputValues[i] = (long)literalValue.UnsignedLong;
					}
				}
				return true;
			}
		}

		// Token: 0x06000E1E RID: 3614 RVA: 0x00025744 File Offset: 0x00023944
		public void \u0001(_IOperatorExpression \u0002)
		{
			if (this.InputValues.Length == 1)
			{
				this.Result = new long?(Math.Abs(this.InputValues[0]));
			}
		}

		// Token: 0x06000E1F RID: 3615 RVA: 0x0002576C File Offset: 0x0002396C
		public void \u0002(_IOperatorExpression \u0002)
		{
			checked
			{
				if (this.InputValues.Length == 2)
				{
					Operator code = \u0002.Code;
					switch (code)
					{
					case Operator.Add:
						break;
					case Operator.Sub:
						goto IL_70;
					case Operator.Mul:
						goto IL_8D;
					case Operator.Div:
						goto IL_AA;
					case Operator.Mod:
						if (this.InputValues[1] == 0L)
						{
							this.Result = new long?(0L);
							return;
						}
						this.Result = new long?(this.InputValues[0] % this.InputValues[1]);
						return;
					default:
						switch (code)
						{
						case Operator.Plus:
							break;
						case Operator.Minus:
							goto IL_70;
						case Operator.Times:
							goto IL_8D;
						case Operator.Power:
							return;
						case Operator.Divide:
							goto IL_AA;
						default:
							return;
						}
						break;
					}
					this.Result = new long?(this.InputValues[0] + this.InputValues[1]);
					return;
					IL_70:
					this.Result = new long?(this.InputValues[0] - this.InputValues[1]);
					return;
					IL_8D:
					this.Result = new long?(this.InputValues[0] * this.InputValues[1]);
					return;
					IL_AA:
					if (this.InputValues[1] != 0L)
					{
						this.Result = new long?(this.InputValues[0] / this.InputValues[1]);
						return;
					}
				}
			}
		}

		// Token: 0x06000E20 RID: 3616 RVA: 0x00025880 File Offset: 0x00023A80
		public void \u0003(_IOperatorExpression \u0002)
		{
			checked
			{
				if (\u0002.Code == Operator.Not && \u0002.Type != null && this.InputValues.Length == 1)
				{
					int num = TypeTable.GetSize(\u0002.Type.Class, null) * 8;
					this.Result = new long?(~this.InputValues[0]);
					if (num < 64)
					{
						this.Result = this.Result << 64 - num >> 64 - num;
						return;
					}
				}
				else if (this.InputValues.Length == 2)
				{
					Operator code = \u0002.Code;
					switch (code)
					{
					case Operator.And:
						break;
					case Operator.AndN:
					case Operator.OrN:
						return;
					case Operator.Or:
						goto IL_123;
					case Operator.Xor:
						this.Result = new long?((this.InputValues[0] & ~this.InputValues[1]) | (~this.InputValues[0] & this.InputValues[1]));
						return;
					default:
						if (code != Operator.Ampersand)
						{
							if (code != Operator.VerticalLine)
							{
								return;
							}
							goto IL_123;
						}
						break;
					}
					this.Result = new long?(this.InputValues[0] & this.InputValues[1]);
					return;
					IL_123:
					this.Result = new long?(this.InputValues[0] | this.InputValues[1]);
					return;
				}
			}
		}

		// Token: 0x06000E21 RID: 3617 RVA: 0x00025A00 File Offset: 0x00023C00
		public void \u0004(_IOperatorExpression \u0002)
		{
			if (this.InputValues.Length == 3 && \u0002.Code == Operator.Limit)
			{
				this.Result = new long?(Math.Min(Math.Max(this.InputValues[0], this.InputValues[1]), this.InputValues[2]));
				return;
			}
			if (\u0002.Code == Operator.Max && this.InputValues.Length == 2)
			{
				this.Result = new long?(Math.Max(this.InputValues[0], this.InputValues[1]));
				return;
			}
			if (\u0002.Code == Operator.Min && this.InputValues.Length == 2)
			{
				this.Result = new long?(Math.Min(this.InputValues[0], this.InputValues[1]));
			}
		}

		// Token: 0x06000E22 RID: 3618 RVA: 0x00025ABC File Offset: 0x00023CBC
		public void \u0005(_IOperatorExpression \u0002)
		{
			if (this.InputValues.Length == 2 && \u0002.Type != null)
			{
				int num = TypeTable.GetSize(\u0002.Type.Class, null) * 8;
				checked
				{
					switch (\u0002.Code)
					{
					case Operator.Rol:
					{
						ulong num2 = (ulong)this.InputValues[0];
						ulong num3 = (ulong)this.InputValues[1];
						num3 %= (ulong)num;
						num2 = (num2 << (int)num3 | num2 >> num - (int)num3);
						this.\u0001(num, num2);
						return;
					}
					case Operator.Ror:
					{
						ulong num2 = (ulong)this.InputValues[0];
						ulong num3 = (ulong)this.InputValues[1];
						num3 %= (ulong)num;
						num2 = (num2 >> (int)num3 | num2 << num - (int)num3);
						this.\u0001(num, num2);
						return;
					}
					case Operator.Shl:
						this.Result = new long?(this.InputValues[0] << (int)this.InputValues[1]);
						if (num < 64)
						{
							this.Result = this.Result << 64 - num >> 64 - num;
							return;
						}
						break;
					case Operator.Shr:
						this.Result = new long?(this.InputValues[0] >> (int)this.InputValues[1]);
						if (num < 64)
						{
							this.Result = this.Result << 64 - num >> 64 - num;
						}
						break;
					default:
						return;
					}
				}
			}
		}

		// Token: 0x06000E23 RID: 3619 RVA: 0x00025CA4 File Offset: 0x00023EA4
		private void \u0001(int \u0002, ulong \u0003)
		{
			if (\u0002 < 64)
			{
				\u0003 = \u0003 << 64 - \u0002 >> 64 - \u0002;
			}
			if (\u0002 <= 16)
			{
				if (\u0002 == 8)
				{
					sbyte b = (sbyte)\u0003;
					this.Result = new long?((long)b);
					return;
				}
				if (\u0002 != 16)
				{
					return;
				}
				short num = (short)\u0003;
				this.Result = new long?((long)num);
				return;
			}
			else
			{
				if (\u0002 == 32)
				{
					int num2 = (int)\u0003;
					this.Result = new long?((long)num2);
					return;
				}
				if (\u0002 != 64)
				{
					return;
				}
				long value = (long)\u0003;
				this.Result = new long?(value);
				return;
			}
		}

		// Token: 0x0400025B RID: 603
		[CompilerGenerated]
		private long[] \u0001;

		// Token: 0x0400025C RID: 604
		[CompilerGenerated]
		private long? \u0001;
	}
}
