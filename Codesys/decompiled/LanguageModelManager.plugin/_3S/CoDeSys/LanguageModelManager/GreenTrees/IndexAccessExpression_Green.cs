using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200020A RID: 522
	internal class IndexAccessExpression_Green : Expression_Green, _IIndexAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IIndexAccessExpression
	{
		// Token: 0x06002363 RID: 9059 RVA: 0x0005BB4E File Offset: 0x0005AB4E
		internal IndexAccessExpression_Green(_IExpression expBase, _IExpression[] expAccesses)
		{
			this.m_expVar = expBase;
			this.m_expAccesses = expAccesses;
		}

		// Token: 0x170009F1 RID: 2545
		// (get) Token: 0x06002364 RID: 9060 RVA: 0x0005BB64 File Offset: 0x0005AB64
		public IExpression Var
		{
			get
			{
				return this._Var;
			}
		}

		// Token: 0x170009F2 RID: 2546
		// (get) Token: 0x06002365 RID: 9061 RVA: 0x0005BB6C File Offset: 0x0005AB6C
		// (set) Token: 0x06002366 RID: 9062 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Var
		{
			get
			{
				if (this.m_expVar == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expVar;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x170009F3 RID: 2547
		// (get) Token: 0x06002367 RID: 9063 RVA: 0x0005BB84 File Offset: 0x0005AB84
		public IExpression[] Accesses
		{
			get
			{
				return this.m_expAccesses;
			}
		}

		// Token: 0x170009F4 RID: 2548
		// (get) Token: 0x06002368 RID: 9064 RVA: 0x0005BB99 File Offset: 0x0005AB99
		public ICollection<_IExpression> _Accesses
		{
			get
			{
				return this.m_expAccesses;
			}
		}

		// Token: 0x170009F5 RID: 2549
		// (get) Token: 0x06002369 RID: 9065 RVA: 0x0005BBA1 File Offset: 0x0005ABA1
		public int NumAccesses
		{
			get
			{
				return this.m_expAccesses.Length;
			}
		}

		// Token: 0x0600236A RID: 9066 RVA: 0x0005BBAB File Offset: 0x0005ABAB
		public _IExpression GetAccess(int i)
		{
			return this.m_expAccesses[i];
		}

		// Token: 0x170009F6 RID: 2550
		public _IExpression this[int i]
		{
			get
			{
				return this.m_expAccesses[i];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600236D RID: 9069 RVA: 0x0005A471 File Offset: 0x00059471
		public void AddAccess(_IExpression expAcc)
		{
			throw new NotSupportedException();
		}

		// Token: 0x0600236E RID: 9070 RVA: 0x0000F4E5 File Offset: 0x0000E4E5
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600236F RID: 9071 RVA: 0x0000F4F7 File Offset: 0x0000E4F7
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006C6 RID: 1734
		private readonly _IExpression m_expVar;

		// Token: 0x040006C7 RID: 1735
		private readonly _IExpression[] m_expAccesses;
	}
}
