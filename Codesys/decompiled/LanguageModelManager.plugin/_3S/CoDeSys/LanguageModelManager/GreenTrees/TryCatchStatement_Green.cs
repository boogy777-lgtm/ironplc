using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001EA RID: 490
	internal class TryCatchStatement_Green : Statement_Green, _ITryCatchStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x06002224 RID: 8740 RVA: 0x0005AA08 File Offset: 0x00059A08
		internal TryCatchStatement_Green(_ISequenceStatement sTry, _ISequenceStatement sCatch, _ISequenceStatement sFinally, _IExpression exception)
		{
			this._seqTry = sTry;
			this._seqCatch = sCatch;
			this._seqFinally = sFinally;
			this._Exception = exception;
		}

		// Token: 0x17000976 RID: 2422
		// (get) Token: 0x06002225 RID: 8741 RVA: 0x0005AA2D File Offset: 0x00059A2D
		public ISequenceStatement3 Try
		{
			get
			{
				return this._seqTry;
			}
		}

		// Token: 0x17000977 RID: 2423
		// (get) Token: 0x06002226 RID: 8742 RVA: 0x0005AA35 File Offset: 0x00059A35
		public ISequenceStatement3 Catch
		{
			get
			{
				return this._seqCatch;
			}
		}

		// Token: 0x17000978 RID: 2424
		// (get) Token: 0x06002227 RID: 8743 RVA: 0x0005AA3D File Offset: 0x00059A3D
		public ISequenceStatement3 Finally
		{
			get
			{
				return this._seqFinally;
			}
		}

		// Token: 0x17000979 RID: 2425
		// (get) Token: 0x06002228 RID: 8744 RVA: 0x0005AA2D File Offset: 0x00059A2D
		// (set) Token: 0x06002229 RID: 8745 RVA: 0x0005A471 File Offset: 0x00059471
		public _ISequenceStatement _Try
		{
			get
			{
				return this._seqTry;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x1700097A RID: 2426
		// (get) Token: 0x0600222A RID: 8746 RVA: 0x0005AA35 File Offset: 0x00059A35
		// (set) Token: 0x0600222B RID: 8747 RVA: 0x0005A471 File Offset: 0x00059471
		public _ISequenceStatement _Catch
		{
			get
			{
				return this._seqCatch;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x1700097B RID: 2427
		// (get) Token: 0x0600222C RID: 8748 RVA: 0x0005AA3D File Offset: 0x00059A3D
		// (set) Token: 0x0600222D RID: 8749 RVA: 0x0005A471 File Offset: 0x00059471
		public _ISequenceStatement _Finally
		{
			get
			{
				return this._seqFinally;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x1700097C RID: 2428
		// (get) Token: 0x0600222E RID: 8750 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x0600222F RID: 8751 RVA: 0x0005A471 File Offset: 0x00059471
		public _ISequenceStatement _ReplacedSequence
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x1700097D RID: 2429
		// (get) Token: 0x06002230 RID: 8752 RVA: 0x0005AA45 File Offset: 0x00059A45
		// (set) Token: 0x06002231 RID: 8753 RVA: 0x0005AA4D File Offset: 0x00059A4D
		public _IExpression _Exception { get; set; }

		// Token: 0x1700097E RID: 2430
		// (get) Token: 0x06002232 RID: 8754 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x06002233 RID: 8755 RVA: 0x0005A471 File Offset: 0x00059471
		public _ISubRoutineStatement Subroutine
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x1700097F RID: 2431
		// (get) Token: 0x06002234 RID: 8756 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x06002235 RID: 8757 RVA: 0x0005A471 File Offset: 0x00059471
		public int Index
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002236 RID: 8758 RVA: 0x0005AA56 File Offset: 0x00059A56
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitor2)
			{
				(visitor as IExprementVisitor2).visit(this);
				return;
			}
			this.DefaultTraverse(visitor);
		}

		// Token: 0x06002237 RID: 8759 RVA: 0x0005AA74 File Offset: 0x00059A74
		public void DefaultTraverse(IExprementVisitor visitor)
		{
			if (this._Exception != null)
			{
				this._Exception.Accept(visitor);
			}
			if (this._Try != null)
			{
				this._Try.Accept(visitor);
			}
			if (this._Catch != null)
			{
				this._Catch.Accept(visitor);
			}
			if (this._Finally != null)
			{
				this._Finally.Accept(visitor);
			}
		}

		// Token: 0x06002238 RID: 8760 RVA: 0x0005AAD4 File Offset: 0x00059AD4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			if (this.Try != null)
			{
				(this.Try as _IStatement).AcceptVisitor(visitor);
			}
			if (this.Catch != null)
			{
				(this.Catch as _IStatement).AcceptVisitor(visitor);
			}
			if (this.Finally != null)
			{
				(this.Finally as _IStatement).AcceptVisitor(visitor);
			}
		}

		// Token: 0x0400068D RID: 1677
		private readonly _ISequenceStatement _seqTry;

		// Token: 0x0400068E RID: 1678
		private readonly _ISequenceStatement _seqCatch;

		// Token: 0x0400068F RID: 1679
		private readonly _ISequenceStatement _seqFinally;
	}
}
