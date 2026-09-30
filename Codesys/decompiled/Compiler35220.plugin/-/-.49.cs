using System;
using System.Runtime.CompilerServices;
using \u0008;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0017
{
	// Token: 0x020000AD RID: 173
	internal sealed class \u0002 : \u0082.\u0003, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor, global::\u0008.\u0004
	{
		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x06000DFF RID: 3583 RVA: 0x00024E58 File Offset: 0x00023058
		// (set) Token: 0x06000E00 RID: 3584 RVA: 0x00024E60 File Offset: 0x00023060
		private double[] InputValues { get; set; }

		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x06000E01 RID: 3585 RVA: 0x00024E6C File Offset: 0x0002306C
		// (set) Token: 0x06000E02 RID: 3586 RVA: 0x00024E74 File Offset: 0x00023074
		private double? Result { get; set; }

		// Token: 0x06000E03 RID: 3587 RVA: 0x00024E80 File Offset: 0x00023080
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

		// Token: 0x06000E04 RID: 3588 RVA: 0x00024EFC File Offset: 0x000230FC
		private bool \u0001(ILiteralValue[] \u0002)
		{
			this.InputValues = new double[\u0002.Length];
			checked
			{
				for (int i = 0; i < \u0002.Length; i++)
				{
					ILiteralValue literalValue = \u0002[i];
					if (literalValue.KindOf == KindOfLiteral.SignedInteger)
					{
						this.InputValues[i] = (double)literalValue.SignedLong;
					}
					else if (literalValue.KindOf == KindOfLiteral.UnsignedInteger)
					{
						this.InputValues[i] = literalValue.UnsignedLong;
					}
					else
					{
						if (literalValue.KindOf != KindOfLiteral.Float)
						{
							return false;
						}
						this.InputValues[i] = literalValue.Float;
					}
				}
				return true;
			}
		}

		// Token: 0x06000E05 RID: 3589 RVA: 0x00024F7C File Offset: 0x0002317C
		public void \u0001(_IOperatorExpression \u0002)
		{
			if (this.InputValues.Length == 1)
			{
				this.Result = new double?(Math.Abs(this.InputValues[0]));
			}
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x00024FA4 File Offset: 0x000231A4
		public void \u0002(_IOperatorExpression \u0002)
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
					this.Result = null;
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
				this.Result = new double?(this.InputValues[0] + this.InputValues[1]);
				return;
				IL_70:
				this.Result = new double?(this.InputValues[0] - this.InputValues[1]);
				return;
				IL_8D:
				this.Result = new double?(this.InputValues[0] * this.InputValues[1]);
				return;
				IL_AA:
				if (this.InputValues[1] != 0.0)
				{
					this.Result = new double?(this.InputValues[0] / this.InputValues[1]);
					return;
				}
			}
		}

		// Token: 0x06000E07 RID: 3591 RVA: 0x0002509C File Offset: 0x0002329C
		public override void visitTrigonometrics(_IOperatorExpression op)
		{
			Operator code = op.Code;
			switch (code)
			{
			case Operator.Exp:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Exp(this.InputValues[0]));
					return;
				}
				return;
			case Operator.Expt:
				break;
			case Operator.Sqrt:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Sqrt(this.InputValues[0]));
					return;
				}
				return;
			case Operator.Ln:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Log(this.InputValues[0]));
					return;
				}
				return;
			case Operator.Log:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Log10(this.InputValues[0]));
					return;
				}
				return;
			case Operator.Sin:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Sin(this.InputValues[0]));
					return;
				}
				return;
			case Operator.Cos:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Cos(this.InputValues[0]));
					return;
				}
				return;
			case Operator.Tan:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Tan(this.InputValues[0]));
					return;
				}
				return;
			case Operator.ASin:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Asin(this.InputValues[0]));
					return;
				}
				return;
			case Operator.ACos:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Acos(this.InputValues[0]));
					return;
				}
				return;
			case Operator.ATan:
				if (this.InputValues.Length == 1)
				{
					this.Result = new double?(Math.Atan(this.InputValues[0]));
					return;
				}
				return;
			default:
				if (code != Operator.Power)
				{
					return;
				}
				break;
			}
			if (this.InputValues.Length == 2)
			{
				this.Result = new double?(Math.Pow(this.InputValues[0], this.InputValues[1]));
				return;
			}
		}

		// Token: 0x06000E08 RID: 3592 RVA: 0x0002529C File Offset: 0x0002349C
		public void \u0003(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06000E09 RID: 3593 RVA: 0x000252A0 File Offset: 0x000234A0
		public void \u0004(_IOperatorExpression \u0002)
		{
			if (this.InputValues.Length == 3 && \u0002.Code == Operator.Limit)
			{
				this.Result = new double?(Math.Min(Math.Max(this.InputValues[0], this.InputValues[1]), this.InputValues[2]));
				return;
			}
			if (\u0002.Code == Operator.Max && this.InputValues.Length == 2)
			{
				this.Result = new double?(Math.Max(this.InputValues[0], this.InputValues[1]));
				return;
			}
			if (\u0002.Code == Operator.Min && this.InputValues.Length == 2)
			{
				this.Result = new double?(Math.Min(this.InputValues[0], this.InputValues[1]));
			}
		}

		// Token: 0x06000E0A RID: 3594 RVA: 0x0002535C File Offset: 0x0002355C
		public void \u0005(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x04000258 RID: 600
		[CompilerGenerated]
		private double[] \u0001;

		// Token: 0x04000259 RID: 601
		[CompilerGenerated]
		private double? \u0001;
	}
}
