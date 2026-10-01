using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001FC RID: 508
	public abstract class OperandList : IList<_IExpression>, ICollection<_IExpression>, IEnumerable<_IExpression>, IEnumerable
	{
		// Token: 0x060022C2 RID: 8898 RVA: 0x0005B5CE File Offset: 0x0005A5CE
		public static OperandList CreateOperandList(_IExpression[] operands)
		{
			if (operands.Length == 1)
			{
				return new OperandList.UnaryExpressionList(operands[0]);
			}
			if (operands.Length == 2)
			{
				return new OperandList.BinaryExpressionList(operands[0], operands[1]);
			}
			return new OperandList.MultiExpressionList(operands);
		}

		// Token: 0x170009B4 RID: 2484
		// (get) Token: 0x060022C3 RID: 8899
		public abstract int Count { get; }

		// Token: 0x170009B5 RID: 2485
		// (get) Token: 0x060022C4 RID: 8900 RVA: 0x00005E58 File Offset: 0x00004E58
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170009B6 RID: 2486
		public abstract _IExpression this[int index]
		{
			get;
			set;
		}

		// Token: 0x060022C7 RID: 8903
		public abstract IEnumerator<_IExpression> GetEnumerator();

		// Token: 0x060022C8 RID: 8904 RVA: 0x0005B5F7 File Offset: 0x0005A5F7
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x060022C9 RID: 8905 RVA: 0x0005B600 File Offset: 0x0005A600
		public int IndexOf(_IExpression item)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (item == this[i])
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060022CA RID: 8906 RVA: 0x0005A471 File Offset: 0x00059471
		public void Insert(int index, _IExpression item)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022CB RID: 8907 RVA: 0x0005A471 File Offset: 0x00059471
		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022CC RID: 8908 RVA: 0x0005A471 File Offset: 0x00059471
		public void Add(_IExpression item)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022CD RID: 8909 RVA: 0x0005A471 File Offset: 0x00059471
		public void Clear()
		{
			throw new NotSupportedException();
		}

		// Token: 0x060022CE RID: 8910 RVA: 0x0005B62C File Offset: 0x0005A62C
		public bool Contains(_IExpression item)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (item == this[i])
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060022CF RID: 8911 RVA: 0x0005B658 File Offset: 0x0005A658
		public void CopyTo(_IExpression[] array, int arrayIndex)
		{
			for (int i = 0; i < this.Count; i++)
			{
				array[arrayIndex + i] = this[i];
			}
		}

		// Token: 0x060022D0 RID: 8912 RVA: 0x0005A471 File Offset: 0x00059471
		public bool Remove(_IExpression item)
		{
			throw new NotSupportedException();
		}

		// Token: 0x020002CE RID: 718
		[SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Nothing to do in Dispose but IEnumerator wants to have it")]
		private abstract class ExpressionEnumerator : IEnumerator<_IExpression>, IDisposable, IEnumerator
		{
			// Token: 0x17000C2E RID: 3118
			// (get) Token: 0x06002C57 RID: 11351
			public abstract _IExpression Current { get; }

			// Token: 0x17000C2F RID: 3119
			// (get) Token: 0x06002C58 RID: 11352 RVA: 0x00074AD7 File Offset: 0x00073AD7
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			// Token: 0x06002C59 RID: 11353
			public abstract void Dispose();

			// Token: 0x06002C5A RID: 11354
			public abstract bool MoveNext();

			// Token: 0x06002C5B RID: 11355
			public abstract void Reset();
		}

		// Token: 0x020002CF RID: 719
		private class UnaryExpressionList : OperandList
		{
			// Token: 0x06002C5D RID: 11357 RVA: 0x00074ADF File Offset: 0x00073ADF
			public UnaryExpressionList(_IExpression exp)
			{
				this._exp = exp;
			}

			// Token: 0x17000C30 RID: 3120
			public override _IExpression this[int index]
			{
				get
				{
					return this._exp;
				}
				set
				{
					this._exp = value;
				}
			}

			// Token: 0x17000C31 RID: 3121
			// (get) Token: 0x06002C60 RID: 11360 RVA: 0x00005E58 File Offset: 0x00004E58
			public override int Count
			{
				get
				{
					return 1;
				}
			}

			// Token: 0x06002C61 RID: 11361 RVA: 0x00074AFF File Offset: 0x00073AFF
			public override IEnumerator<_IExpression> GetEnumerator()
			{
				return new OperandList.UnaryExpressionEnumerator(this._exp);
			}

			// Token: 0x040008F4 RID: 2292
			private _IExpression _exp;
		}

		// Token: 0x020002D0 RID: 720
		private class UnaryExpressionEnumerator : OperandList.ExpressionEnumerator
		{
			// Token: 0x06002C62 RID: 11362 RVA: 0x00074B0C File Offset: 0x00073B0C
			public UnaryExpressionEnumerator(_IExpression exp)
			{
				this._exp = exp;
			}

			// Token: 0x17000C32 RID: 3122
			// (get) Token: 0x06002C63 RID: 11363 RVA: 0x00074B22 File Offset: 0x00073B22
			public override _IExpression Current
			{
				get
				{
					return this._exp;
				}
			}

			// Token: 0x06002C64 RID: 11364 RVA: 0x00074B2A File Offset: 0x00073B2A
			public override bool MoveNext()
			{
				this._i++;
				return this._i == 0;
			}

			// Token: 0x06002C65 RID: 11365 RVA: 0x00003AE9 File Offset: 0x00002AE9
			public override void Reset()
			{
			}

			// Token: 0x06002C66 RID: 11366 RVA: 0x00003AE9 File Offset: 0x00002AE9
			public override void Dispose()
			{
			}

			// Token: 0x040008F5 RID: 2293
			private readonly _IExpression _exp;

			// Token: 0x040008F6 RID: 2294
			private int _i = -1;
		}

		// Token: 0x020002D1 RID: 721
		private class BinaryExpressionList : OperandList
		{
			// Token: 0x06002C67 RID: 11367 RVA: 0x00074B43 File Offset: 0x00073B43
			public BinaryExpressionList(_IExpression exp1, _IExpression exp2)
			{
				this._exp1 = exp1;
				this._exp2 = exp2;
			}

			// Token: 0x17000C33 RID: 3123
			// (get) Token: 0x06002C68 RID: 11368 RVA: 0x0004EFBE File Offset: 0x0004DFBE
			public override int Count
			{
				get
				{
					return 2;
				}
			}

			// Token: 0x17000C34 RID: 3124
			public override _IExpression this[int index]
			{
				get
				{
					if (index != 0)
					{
						return this._exp2;
					}
					return this._exp1;
				}
				set
				{
					if (index == 0)
					{
						this._exp1 = value;
						return;
					}
					if (index == 1)
					{
						this._exp2 = value;
					}
				}
			}

			// Token: 0x06002C6B RID: 11371 RVA: 0x00074B83 File Offset: 0x00073B83
			public override IEnumerator<_IExpression> GetEnumerator()
			{
				return new OperandList.BinaryExpressionEnumerator(this._exp1, this._exp2);
			}

			// Token: 0x040008F7 RID: 2295
			private _IExpression _exp1;

			// Token: 0x040008F8 RID: 2296
			private _IExpression _exp2;
		}

		// Token: 0x020002D2 RID: 722
		private class BinaryExpressionEnumerator : OperandList.ExpressionEnumerator
		{
			// Token: 0x06002C6C RID: 11372 RVA: 0x00074B96 File Offset: 0x00073B96
			public BinaryExpressionEnumerator(_IExpression exp1, _IExpression exp2)
			{
				this._exp1 = exp1;
				this._exp2 = exp2;
			}

			// Token: 0x17000C35 RID: 3125
			// (get) Token: 0x06002C6D RID: 11373 RVA: 0x00074BB3 File Offset: 0x00073BB3
			public override _IExpression Current
			{
				get
				{
					if (this._i != 0)
					{
						return this._exp2;
					}
					return this._exp1;
				}
			}

			// Token: 0x06002C6E RID: 11374 RVA: 0x00074BCA File Offset: 0x00073BCA
			public override bool MoveNext()
			{
				this._i++;
				return this._i < 2;
			}

			// Token: 0x06002C6F RID: 11375 RVA: 0x00074BE3 File Offset: 0x00073BE3
			public override void Reset()
			{
				this._i = -1;
			}

			// Token: 0x06002C70 RID: 11376 RVA: 0x00003AE9 File Offset: 0x00002AE9
			public override void Dispose()
			{
			}

			// Token: 0x040008F9 RID: 2297
			private readonly _IExpression _exp1;

			// Token: 0x040008FA RID: 2298
			private readonly _IExpression _exp2;

			// Token: 0x040008FB RID: 2299
			private int _i = -1;
		}

		// Token: 0x020002D3 RID: 723
		private class MultiExpressionList : OperandList
		{
			// Token: 0x06002C71 RID: 11377 RVA: 0x00074BEC File Offset: 0x00073BEC
			public MultiExpressionList(_IExpression[] exprs)
			{
				this._exprs = exprs;
			}

			// Token: 0x17000C36 RID: 3126
			// (get) Token: 0x06002C72 RID: 11378 RVA: 0x00074BFB File Offset: 0x00073BFB
			public override int Count
			{
				get
				{
					return this._exprs.Length;
				}
			}

			// Token: 0x17000C37 RID: 3127
			public override _IExpression this[int index]
			{
				get
				{
					return this._exprs[index];
				}
				set
				{
					this._exprs[index] = value;
				}
			}

			// Token: 0x06002C75 RID: 11381 RVA: 0x00074C1A File Offset: 0x00073C1A
			public override IEnumerator<_IExpression> GetEnumerator()
			{
				return new OperandList.MultiExpressionEnumerator(this._exprs);
			}

			// Token: 0x040008FC RID: 2300
			private readonly _IExpression[] _exprs;
		}

		// Token: 0x020002D4 RID: 724
		private class MultiExpressionEnumerator : OperandList.ExpressionEnumerator
		{
			// Token: 0x06002C76 RID: 11382 RVA: 0x00074C27 File Offset: 0x00073C27
			public MultiExpressionEnumerator(_IExpression[] exprs)
			{
				this._exprs = exprs;
			}

			// Token: 0x17000C38 RID: 3128
			// (get) Token: 0x06002C77 RID: 11383 RVA: 0x00074C3D File Offset: 0x00073C3D
			public override _IExpression Current
			{
				get
				{
					return this._exprs[this._i];
				}
			}

			// Token: 0x06002C78 RID: 11384 RVA: 0x00003AE9 File Offset: 0x00002AE9
			public override void Dispose()
			{
			}

			// Token: 0x06002C79 RID: 11385 RVA: 0x00074C4C File Offset: 0x00073C4C
			public override bool MoveNext()
			{
				this._i++;
				return this._i < this._exprs.Length;
			}

			// Token: 0x06002C7A RID: 11386 RVA: 0x00074C6C File Offset: 0x00073C6C
			public override void Reset()
			{
				this._i = -1;
			}

			// Token: 0x040008FD RID: 2301
			private readonly _IExpression[] _exprs;

			// Token: 0x040008FE RID: 2302
			private int _i = -1;
		}
	}
}
