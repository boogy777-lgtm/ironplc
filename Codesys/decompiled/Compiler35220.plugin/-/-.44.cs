using System;
using System.Runtime.CompilerServices;
using \u0008;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u001C
{
	// Token: 0x020000A2 RID: 162
	internal sealed class \u0002 : \u0082.\u0003, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor, global::\u0008.\u0004
	{
		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x06000D43 RID: 3395 RVA: 0x00022724 File Offset: 0x00020924
		// (set) Token: 0x06000D44 RID: 3396 RVA: 0x0002272C File Offset: 0x0002092C
		private bool? Result { get; set; }

		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x06000D45 RID: 3397 RVA: 0x00022738 File Offset: 0x00020938
		// (set) Token: 0x06000D46 RID: 3398 RVA: 0x00022740 File Offset: 0x00020940
		private ILiteralValue[] InputValue { get; set; }

		// Token: 0x06000D47 RID: 3399 RVA: 0x0002274C File Offset: 0x0002094C
		public ILiteralValue \u0001(_IOperatorExpression \u0002, ILiteralValue[] \u0003, out EConstantFoldingResult \u0004)
		{
			\u0004 = EConstantFoldingResult.None;
			try
			{
				this.InputValue = \u0003;
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

		// Token: 0x06000D48 RID: 3400 RVA: 0x000227C0 File Offset: 0x000209C0
		private static bool? \u0001<\u0001>(\u0001 \u0002, \u0001 \u0003, Operator \u0004) where \u0001 : IComparable
		{
			switch (\u0004)
			{
			case Operator.Eq:
				break;
			case Operator.Ne:
				goto IL_DC;
			case Operator.Ge:
				goto IL_66;
			case Operator.Gt:
				goto IL_85;
			case Operator.Le:
				goto IL_A1;
			case Operator.Lt:
				goto IL_C0;
			default:
				switch (\u0004)
				{
				case Operator.Less:
					goto IL_C0;
				case Operator.Greater:
					goto IL_85;
				case Operator.LessEqual:
					goto IL_A1;
				case Operator.GreaterEqual:
					goto IL_66;
				case Operator.Equal:
					break;
				case Operator.NotEqual:
					goto IL_DC;
				default:
					return null;
				}
				break;
			}
			return new bool?(\u0002.Equals(\u0003));
			IL_66:
			return new bool?(\u0002.CompareTo(\u0003) >= 0);
			IL_85:
			return new bool?(\u0002.CompareTo(\u0003) > 0);
			IL_A1:
			return new bool?(\u0002.CompareTo(\u0003) <= 0);
			IL_C0:
			return new bool?(\u0002.CompareTo(\u0003) < 0);
			IL_DC:
			return new bool?(!\u0002.Equals(\u0003));
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x000228D0 File Offset: 0x00020AD0
		public void \u0001(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06000D4A RID: 3402 RVA: 0x000228D4 File Offset: 0x00020AD4
		public void \u0002(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06000D4B RID: 3403 RVA: 0x000228D8 File Offset: 0x00020AD8
		private void \u0001(out bool? \u0002, out bool? \u0003)
		{
			\u0002 = null;
			\u0003 = null;
			if (this.InputValue.Length >= 1 && this.InputValue[0].KindOf == KindOfLiteral.Bool)
			{
				\u0002 = new bool?(this.InputValue[0].Bool);
			}
			if (this.InputValue.Length >= 2 && this.InputValue[1].KindOf == KindOfLiteral.Bool)
			{
				\u0003 = new bool?(this.InputValue[1].Bool);
			}
		}

		// Token: 0x06000D4C RID: 3404 RVA: 0x0002295C File Offset: 0x00020B5C
		public void \u0003(_IOperatorExpression \u0002)
		{
			bool? flag;
			bool? flag2;
			this.\u0001(out flag, out flag2);
			if (\u0002.Code == Operator.Not && flag != null && flag2 == null)
			{
				this.Result = new bool?(!flag.Value);
				return;
			}
			if (flag != null && flag2 != null)
			{
				Operator code = \u0002.Code;
				if (code <= Operator.Ampersand)
				{
					switch (code)
					{
					case Operator.And:
						break;
					case Operator.AndN:
					case Operator.OrN:
						return;
					case Operator.Or:
						goto IL_C4;
					case Operator.Xor:
						this.Result = new bool?((flag.Value && !flag2.Value) || (!flag.Value && flag2.Value));
						return;
					default:
						if (code != Operator.Ampersand)
						{
							return;
						}
						break;
					}
				}
				else
				{
					if (code == Operator.VerticalLine)
					{
						goto IL_C4;
					}
					if (code != Operator.And_Then)
					{
						if (code != Operator.Or_Else)
						{
							return;
						}
						goto IL_C4;
					}
				}
				this.Result = new bool?(flag.Value && flag2.Value);
				return;
				IL_C4:
				this.Result = new bool?(flag.Value || flag2.Value);
				return;
			}
		}

		// Token: 0x06000D4D RID: 3405 RVA: 0x00022A80 File Offset: 0x00020C80
		public override void visitComparisons(_IOperatorExpression op)
		{
			if (this.InputValue.Length == 2)
			{
				if (this.InputValue[0].KindOf != this.InputValue[1].KindOf)
				{
					return;
				}
				switch (this.InputValue[0].KindOf)
				{
				case KindOfLiteral.SignedInteger:
				{
					long signedLong = this.InputValue[0].SignedLong;
					long signedLong2 = this.InputValue[1].SignedLong;
					this.Result = \u001C.\u0002.\u0001<long>(signedLong, signedLong2, op.Code);
					return;
				}
				case KindOfLiteral.UnsignedInteger:
				{
					ulong unsignedLong = this.InputValue[0].UnsignedLong;
					ulong unsignedLong2 = this.InputValue[1].UnsignedLong;
					this.Result = \u001C.\u0002.\u0001<ulong>(unsignedLong, unsignedLong2, op.Code);
					return;
				}
				case KindOfLiteral.Float:
				{
					double @float = this.InputValue[0].Float;
					double float2 = this.InputValue[1].Float;
					this.Result = \u001C.\u0002.\u0001<double>(@float, float2, op.Code);
					return;
				}
				case KindOfLiteral.String:
				{
					string @string = this.InputValue[0].String;
					string string2 = this.InputValue[1].String;
					this.Result = \u001C.\u0002.\u0001<string>(@string, string2, op.Code);
					return;
				}
				case KindOfLiteral.Bool:
				{
					bool @bool = this.InputValue[0].Bool;
					bool bool2 = this.InputValue[1].Bool;
					this.Result = \u001C.\u0002.\u0001<bool>(@bool, bool2, op.Code);
					break;
				}
				default:
					return;
				}
			}
		}

		// Token: 0x06000D4E RID: 3406 RVA: 0x00022BE0 File Offset: 0x00020DE0
		public void \u0004(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x06000D4F RID: 3407 RVA: 0x00022BE4 File Offset: 0x00020DE4
		public void \u0005(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x04000236 RID: 566
		[CompilerGenerated]
		private bool? \u0001;

		// Token: 0x04000237 RID: 567
		[CompilerGenerated]
		private ILiteralValue[] \u0001;
	}
}
