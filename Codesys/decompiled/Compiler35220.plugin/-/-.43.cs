using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0016
{
	// Token: 0x020000A0 RID: 160
	internal abstract class \u0002 : \u0003, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor
	{
		// Token: 0x17000427 RID: 1063
		// (get) Token: 0x06000D0D RID: 3341 RVA: 0x000224A8 File Offset: 0x000206A8
		// (set) Token: 0x06000D0E RID: 3342 RVA: 0x000224B0 File Offset: 0x000206B0
		protected long[] InputValues { get; set; }

		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x06000D0F RID: 3343 RVA: 0x000224BC File Offset: 0x000206BC
		// (set) Token: 0x06000D10 RID: 3344 RVA: 0x000224C4 File Offset: 0x000206C4
		protected long? Result { get; set; }

		// Token: 0x06000D11 RID: 3345 RVA: 0x000224D0 File Offset: 0x000206D0
		public void \u0001(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06000D12 RID: 3346 RVA: 0x000224D4 File Offset: 0x000206D4
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
						goto IL_6C;
					case Operator.Mul:
						goto IL_89;
					case Operator.Div:
						goto IL_A6;
					default:
						switch (code)
						{
						case Operator.Plus:
							break;
						case Operator.Minus:
							goto IL_6C;
						case Operator.Times:
							goto IL_89;
						case Operator.Power:
							return;
						case Operator.Divide:
							goto IL_A6;
						default:
							return;
						}
						break;
					}
					this.Result = new long?(this.InputValues[0] + this.InputValues[1]);
					return;
					IL_6C:
					this.Result = new long?(this.InputValues[0] - this.InputValues[1]);
					return;
					IL_89:
					this.Result = new long?(this.InputValues[0] * this.InputValues[1]);
					return;
					IL_A6:
					this.Result = new long?(this.InputValues[0] / this.InputValues[1]);
				}
			}
		}

		// Token: 0x06000D13 RID: 3347 RVA: 0x000225A4 File Offset: 0x000207A4
		public void \u0003(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06000D14 RID: 3348 RVA: 0x000225A8 File Offset: 0x000207A8
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

		// Token: 0x06000D15 RID: 3349 RVA: 0x00022664 File Offset: 0x00020864
		public void \u0005(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x04000234 RID: 564
		[CompilerGenerated]
		private long[] \u0001;

		// Token: 0x04000235 RID: 565
		[CompilerGenerated]
		private long? \u0001;
	}
}
