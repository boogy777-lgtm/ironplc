using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A3 RID: 163
	[TypeGuid("{B0F3614C-EAD1-4877-8AE9-16840CC67A07}")]
	[StorageVersion("3.5.6.40")]
	public class TryCatchStatement : PositionStatement, _ITryCatchStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x060009C4 RID: 2500 RVA: 0x000149DB File Offset: 0x000139DB
		public TryCatchStatement()
		{
		}

		// Token: 0x060009C5 RID: 2501 RVA: 0x000149E3 File Offset: 0x000139E3
		public TryCatchStatement(IToken token) : base(token)
		{
		}

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x060009C6 RID: 2502 RVA: 0x00016845 File Offset: 0x00015845
		public ISequenceStatement3 Try
		{
			get
			{
				return this._seqTry;
			}
		}

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x060009C7 RID: 2503 RVA: 0x0001684D File Offset: 0x0001584D
		public ISequenceStatement3 Catch
		{
			get
			{
				return this._seqCatch;
			}
		}

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x060009C8 RID: 2504 RVA: 0x00016855 File Offset: 0x00015855
		public ISequenceStatement3 Finally
		{
			get
			{
				return this._seqFinally;
			}
		}

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x060009C9 RID: 2505 RVA: 0x00016845 File Offset: 0x00015845
		// (set) Token: 0x060009CA RID: 2506 RVA: 0x0001685D File Offset: 0x0001585D
		public _ISequenceStatement _Try
		{
			get
			{
				return this._seqTry;
			}
			set
			{
				this._seqTry = value;
			}
		}

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x060009CB RID: 2507 RVA: 0x0001684D File Offset: 0x0001584D
		// (set) Token: 0x060009CC RID: 2508 RVA: 0x00016866 File Offset: 0x00015866
		public _ISequenceStatement _Catch
		{
			get
			{
				return this._seqCatch;
			}
			set
			{
				this._seqCatch = value;
			}
		}

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x060009CD RID: 2509 RVA: 0x00016855 File Offset: 0x00015855
		// (set) Token: 0x060009CE RID: 2510 RVA: 0x0001686F File Offset: 0x0001586F
		public _ISequenceStatement _Finally
		{
			get
			{
				return this._seqFinally;
			}
			set
			{
				this._seqFinally = value;
			}
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x060009CF RID: 2511 RVA: 0x00016878 File Offset: 0x00015878
		// (set) Token: 0x060009D0 RID: 2512 RVA: 0x00016880 File Offset: 0x00015880
		public _ISequenceStatement _ReplacedSequence { get; set; }

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x060009D1 RID: 2513 RVA: 0x00016889 File Offset: 0x00015889
		// (set) Token: 0x060009D2 RID: 2514 RVA: 0x00016891 File Offset: 0x00015891
		public _IExpression _Exception
		{
			get
			{
				return this._exexc;
			}
			set
			{
				this._exexc = value;
			}
		}

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x060009D3 RID: 2515 RVA: 0x0001689A File Offset: 0x0001589A
		// (set) Token: 0x060009D4 RID: 2516 RVA: 0x000168A2 File Offset: 0x000158A2
		public _ISubRoutineStatement Subroutine { get; set; }

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x060009D5 RID: 2517 RVA: 0x000168AB File Offset: 0x000158AB
		// (set) Token: 0x060009D6 RID: 2518 RVA: 0x000168B3 File Offset: 0x000158B3
		public int Index
		{
			get
			{
				return this._index;
			}
			set
			{
				this._index = value;
			}
		}

		// Token: 0x060009D7 RID: 2519 RVA: 0x000168BC File Offset: 0x000158BC
		public override void Accept(IExprementVisitor visitor)
		{
			if (visitor is IExprementVisitor2)
			{
				(visitor as IExprementVisitor2).visit(this);
				return;
			}
			this.DefaultTraverse(visitor);
		}

		// Token: 0x060009D8 RID: 2520 RVA: 0x000168DA File Offset: 0x000158DA
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060009D9 RID: 2521 RVA: 0x000168E4 File Offset: 0x000158E4
		public void DefaultTraverse(IExprementVisitor visitor)
		{
			if (this._ReplacedSequence != null)
			{
				this._ReplacedSequence.Accept(visitor);
				return;
			}
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

		// Token: 0x060009DA RID: 2522 RVA: 0x00016958 File Offset: 0x00015958
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			if (this.Try != null)
			{
				(this.Try as Statement).AcceptVisitor(visitor);
			}
			if (this.Catch != null)
			{
				(this.Catch as Statement).AcceptVisitor(visitor);
			}
			if (this.Finally != null)
			{
				(this.Finally as Statement).AcceptVisitor(visitor);
			}
		}

		// Token: 0x060009DB RID: 2523 RVA: 0x000169B0 File Offset: 0x000159B0
		public override _IExprement Duplicate()
		{
			TryCatchStatement tryCatchStatement = new TryCatchStatement();
			this.DuplicateCommon(tryCatchStatement);
			if (this.Try != null)
			{
				tryCatchStatement._Try = (this._Try.Duplicate() as _ISequenceStatement);
			}
			if (this.Catch != null)
			{
				tryCatchStatement._Catch = (this._Catch.Duplicate() as _ISequenceStatement);
			}
			if (this.Finally != null)
			{
				tryCatchStatement._Finally = (this._Finally.Duplicate() as _ISequenceStatement);
			}
			if (this._Exception != null)
			{
				tryCatchStatement._Exception = (this._Exception.Duplicate() as _IExpression);
			}
			return tryCatchStatement;
		}

		// Token: 0x0400015C RID: 348
		[DefaultSerialization("TryBlock")]
		[StorageVersion("3.5.6.40")]
		[StorageDefaultValue(null)]
		private _ISequenceStatement _seqTry;

		// Token: 0x0400015D RID: 349
		[DefaultSerialization("CatchBlock")]
		[StorageVersion("3.5.6.40")]
		[StorageDefaultValue(null)]
		private _ISequenceStatement _seqCatch;

		// Token: 0x0400015E RID: 350
		[DefaultSerialization("FinallyBlock")]
		[StorageVersion("3.5.6.40")]
		[StorageDefaultValue(null)]
		private _ISequenceStatement _seqFinally;

		// Token: 0x0400015F RID: 351
		[DefaultSerialization("ExceptionCode")]
		[StorageVersion("3.5.6.40")]
		[StorageDefaultValue(null)]
		private _IExpression _exexc;

		// Token: 0x04000160 RID: 352
		[DefaultSerialization("FPIndex")]
		[StorageVersion("3.5.6.40")]
		[StorageDefaultValue(0)]
		private int _index;
	}
}
