using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0008;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0004
{
	// Token: 0x020000B4 RID: 180
	internal sealed class \u0001 : \u0082.\u0003, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor, global::\u0008.\u0004
	{
		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x06000E2E RID: 3630 RVA: 0x00025F6C File Offset: 0x0002416C
		// (set) Token: 0x06000E2F RID: 3631 RVA: 0x00025F74 File Offset: 0x00024174
		private ulong[] InputValues { get; set; }

		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x06000E30 RID: 3632 RVA: 0x00025F80 File Offset: 0x00024180
		// (set) Token: 0x06000E31 RID: 3633 RVA: 0x00025F88 File Offset: 0x00024188
		private ulong? Result { get; set; }

		// Token: 0x06000E32 RID: 3634 RVA: 0x00025F94 File Offset: 0x00024194
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
			if (this.Result != null)
			{
				return \u0019.\u0003.\u0001(this.Result.Value);
			}
			return null;
		}

		// Token: 0x06000E33 RID: 3635 RVA: 0x00026010 File Offset: 0x00024210
		private bool \u0001(ILiteralValue[] \u0002)
		{
			this.InputValues = new ulong[\u0002.Length];
			checked
			{
				for (int i = 0; i < \u0002.Length; i++)
				{
					ILiteralValue literalValue = \u0002[i];
					if (literalValue.KindOf == KindOfLiteral.SignedInteger)
					{
						this.InputValues[i] = (ulong)literalValue.SignedLong;
					}
					else
					{
						if (literalValue.KindOf != KindOfLiteral.UnsignedInteger)
						{
							return false;
						}
						this.InputValues[i] = literalValue.UnsignedLong;
					}
				}
				return true;
			}
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x00026074 File Offset: 0x00024274
		public void \u0001(_IOperatorExpression \u0002)
		{
			if (this.InputValues.Length == 1)
			{
				this.Result = new ulong?(this.InputValues[0]);
			}
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x00026094 File Offset: 0x00024294
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
						if (this.InputValues[1] == 0UL)
						{
							this.Result = new ulong?(0UL);
							return;
						}
						this.Result = new ulong?(this.InputValues[0] % this.InputValues[1]);
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
					this.Result = new ulong?(this.InputValues[0] + this.InputValues[1]);
					return;
					IL_70:
					this.Result = new ulong?(this.InputValues[0] - this.InputValues[1]);
					return;
					IL_8D:
					this.Result = new ulong?(this.InputValues[0] * this.InputValues[1]);
					return;
					IL_AA:
					if (this.InputValues[1] != 0UL)
					{
						this.Result = new ulong?(this.InputValues[0] / this.InputValues[1]);
						return;
					}
				}
			}
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x000261A8 File Offset: 0x000243A8
		public void \u0003(_IOperatorExpression \u0002)
		{
			checked
			{
				if (\u0002.Code == Operator.Not && \u0002.Type != null && this.InputValues.Length == 1)
				{
					int num = TypeTable.GetSize(\u0002.Type.Class, null) * 8;
					this.Result = new ulong?(~this.InputValues[0]);
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
						this.Result = new ulong?((this.InputValues[0] & ~this.InputValues[1]) | (~this.InputValues[0] & this.InputValues[1]));
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
					this.Result = new ulong?(this.InputValues[0] & this.InputValues[1]);
					return;
					IL_123:
					this.Result = new ulong?(this.InputValues[0] | this.InputValues[1]);
					return;
				}
			}
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x00026328 File Offset: 0x00024528
		public void \u0004(_IOperatorExpression \u0002)
		{
			if (this.InputValues.Length == 3 && \u0002.Code == Operator.Limit)
			{
				this.Result = new ulong?(Math.Min(Math.Max(this.InputValues[0], this.InputValues[1]), this.InputValues[2]));
				return;
			}
			if (\u0002.Code == Operator.Max && this.InputValues.Length == 2)
			{
				this.Result = new ulong?(Math.Max(this.InputValues[0], this.InputValues[1]));
				return;
			}
			if (\u0002.Code == Operator.Min && this.InputValues.Length == 2)
			{
				this.Result = new ulong?(Math.Min(this.InputValues[0], this.InputValues[1]));
			}
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x000263E4 File Offset: 0x000245E4
		private static bool \u0001(_IOperatorExpression \u0002, out int \u0003)
		{
			\u0003 = -1;
			if (\u0002.Type == null && \u0002._OperandsList.Count == 0)
			{
				return false;
			}
			if (\u0002.Type != null)
			{
				\u0003 = TypeTable.GetSize(\u0002.Type.Class, null) * 8;
				return true;
			}
			if (\u0002._OperandsList.First<_IExpression>().Type != null)
			{
				\u0003 = TypeTable.GetSize(\u0002._OperandsList.First<_IExpression>().Type.Class, null) * 8;
				return true;
			}
			return false;
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x00026460 File Offset: 0x00024660
		public void \u0005(_IOperatorExpression \u0002)
		{
			if (this.InputValues.Length != 2)
			{
				return;
			}
			int num;
			if (!global::\u0004.\u0001.\u0001(\u0002, out num))
			{
				return;
			}
			ulong num2 = this.InputValues[0];
			ulong num3 = this.InputValues[1];
			checked
			{
				switch (\u0002.Code)
				{
				case Operator.Rol:
					num3 %= (ulong)num;
					this.Result = new ulong?(num2 << (int)num3 | num2 >> num - (int)num3);
					if (num < 64)
					{
						this.Result = this.Result << 64 - num >> 64 - num;
						return;
					}
					break;
				case Operator.Ror:
					num3 %= (ulong)num;
					this.Result = new ulong?(num2 >> (int)num3 | num2 << num - (int)num3);
					if (num < 64)
					{
						this.Result = this.Result << 64 - num >> 64 - num;
						return;
					}
					break;
				case Operator.Shl:
					this.Result = new ulong?(this.InputValues[0] << (int)this.InputValues[1]);
					if (num < 64)
					{
						this.Result = this.Result << 64 - num >> 64 - num;
						return;
					}
					break;
				case Operator.Shr:
					this.Result = new ulong?(this.InputValues[0] >> (int)this.InputValues[1]);
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

		// Token: 0x0400025F RID: 607
		[CompilerGenerated]
		private ulong[] \u0001;

		// Token: 0x04000260 RID: 608
		[CompilerGenerated]
		private ulong? \u0001;
	}
}
