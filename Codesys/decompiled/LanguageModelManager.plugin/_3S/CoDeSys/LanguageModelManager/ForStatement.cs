using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000090 RID: 144
	[TypeGuid("{6c7e0062-1086-41b5-ac1a-1fc63e23184b}")]
	[StorageVersion("3.3.0.0")]
	public class ForStatement : PositionStatement, _IForStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IForStatement
	{
		// Token: 0x060008E0 RID: 2272 RVA: 0x000149DB File Offset: 0x000139DB
		public ForStatement()
		{
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x000149E3 File Offset: 0x000139E3
		public ForStatement(IToken token) : base(token)
		{
		}

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x060008E2 RID: 2274 RVA: 0x000150FD File Offset: 0x000140FD
		public IExpression CounterStart
		{
			get
			{
				return this._CounterStart;
			}
		}

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x060008E3 RID: 2275 RVA: 0x00015105 File Offset: 0x00014105
		// (set) Token: 0x060008E4 RID: 2276 RVA: 0x0001511B File Offset: 0x0001411B
		public _IExpression _CounterStart
		{
			get
			{
				if (this.m_assCounterStart == null)
				{
					return new NullExpression();
				}
				return this.m_assCounterStart;
			}
			set
			{
				this.m_assCounterStart = value;
			}
		}

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x060008E5 RID: 2277 RVA: 0x00015124 File Offset: 0x00014124
		public IExpression UpperBound
		{
			get
			{
				return this._UpperBound;
			}
		}

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x060008E6 RID: 2278 RVA: 0x0001512C File Offset: 0x0001412C
		// (set) Token: 0x060008E7 RID: 2279 RVA: 0x00015142 File Offset: 0x00014142
		public _IExpression _UpperBound
		{
			get
			{
				if (this.m_expUpper != null)
				{
					return this.m_expUpper;
				}
				return new NullExpression();
			}
			set
			{
				this.m_expUpper = value;
			}
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x060008E8 RID: 2280 RVA: 0x0001514B File Offset: 0x0001414B
		public IExpression By
		{
			get
			{
				return this._By;
			}
		}

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x060008E9 RID: 2281 RVA: 0x00015153 File Offset: 0x00014153
		// (set) Token: 0x060008EA RID: 2282 RVA: 0x0001515B File Offset: 0x0001415B
		public _IExpression _By
		{
			get
			{
				return this.m_expBy;
			}
			set
			{
				this.m_expBy = value;
			}
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x060008EB RID: 2283 RVA: 0x00015164 File Offset: 0x00014164
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x060008EC RID: 2284 RVA: 0x0001516C File Offset: 0x0001416C
		// (set) Token: 0x060008ED RID: 2285 RVA: 0x00015174 File Offset: 0x00014174
		public _IExpression _Condition
		{
			get
			{
				return this.m_expCondition;
			}
			set
			{
				this.m_expCondition = value;
			}
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x060008EE RID: 2286 RVA: 0x0001517D File Offset: 0x0001417D
		public IExpression Counter
		{
			get
			{
				return this._Counter;
			}
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x060008EF RID: 2287 RVA: 0x00015185 File Offset: 0x00014185
		// (set) Token: 0x060008F0 RID: 2288 RVA: 0x0001518D File Offset: 0x0001418D
		public _IExpression _Counter
		{
			get
			{
				return this.m_assCounter;
			}
			set
			{
				this.m_assCounter = value;
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x060008F1 RID: 2289 RVA: 0x00015196 File Offset: 0x00014196
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x060008F2 RID: 2290 RVA: 0x0001519E File Offset: 0x0001419E
		// (set) Token: 0x060008F3 RID: 2291 RVA: 0x000151B4 File Offset: 0x000141B4
		public _IStatement _Controlled
		{
			get
			{
				if (this.m_stateControlled == null)
				{
					return new NullStatement();
				}
				return this.m_stateControlled;
			}
			set
			{
				this.m_stateControlled = value;
			}
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x000151BD File Offset: 0x000141BD
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x000151C6 File Offset: 0x000141C6
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x000151CF File Offset: 0x000141CF
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x000151D8 File Offset: 0x000141D8
		public override _IExprement Duplicate()
		{
			ForStatement forStatement = new ForStatement();
			this.DuplicateCommon(forStatement);
			if (this.m_assCounterStart != null)
			{
				forStatement.m_assCounterStart = (this.m_assCounterStart.Duplicate() as Expression);
			}
			if (this.m_expUpper != null)
			{
				forStatement.m_expUpper = (this.m_expUpper.Duplicate() as Expression);
			}
			if (this.m_expBy != null)
			{
				forStatement.m_expBy = (this.m_expBy.Duplicate() as Expression);
			}
			if (this.m_stateControlled != null)
			{
				forStatement.m_stateControlled = (this.m_stateControlled.Duplicate() as Statement);
			}
			if (this.m_expCondition != null)
			{
				forStatement.m_expCondition = (this.m_expCondition.Duplicate() as Expression);
			}
			if (this.m_assCounter != null)
			{
				forStatement.m_assCounter = (this.m_assCounter.Duplicate() as Expression);
			}
			return forStatement;
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x000152A7 File Offset: 0x000142A7
		public override IBreakpoint CreateBreakpoint(int nOffset, byte bySize)
		{
			return new Breakpoint(nOffset, this.m_assCounterStart._Position, this.m_assCounterStart.PositionLength);
		}

		// Token: 0x04000131 RID: 305
		[DefaultSerialization("CounterStart")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_assCounterStart;

		// Token: 0x04000132 RID: 306
		[DefaultSerialization("Upper")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expUpper;

		// Token: 0x04000133 RID: 307
		[DefaultSerialization("By")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expBy;

		// Token: 0x04000134 RID: 308
		[DefaultSerialization("Controlled")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_stateControlled;

		// Token: 0x04000135 RID: 309
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCondition;

		// Token: 0x04000136 RID: 310
		[DefaultSerialization("CounterAssign")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_assCounter;
	}
}
