using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200009F RID: 159
	[TypeGuid("{c6dbf46e-6569-4e0a-b092-e91ff272ed57}")]
	[StorageVersion("3.3.0.0")]
	public class RepeatStatement : PositionStatement, _IRepeatStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IRepeatStatement
	{
		// Token: 0x06000996 RID: 2454 RVA: 0x000149DB File Offset: 0x000139DB
		public RepeatStatement()
		{
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x000163BB File Offset: 0x000153BB
		internal RepeatStatement(_IExpression expCond, _IStatement stateControlled)
		{
			this.m_expCond = expCond;
			this.m_stateControlled = stateControlled;
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x000163D1 File Offset: 0x000153D1
		internal RepeatStatement(_IExpression expCond, _IStatement stateControlled, IToken token) : base(token)
		{
			this.m_expCond = expCond;
			this.m_stateControlled = stateControlled;
		}

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06000999 RID: 2457 RVA: 0x000163E8 File Offset: 0x000153E8
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x0600099A RID: 2458 RVA: 0x000163F0 File Offset: 0x000153F0
		// (set) Token: 0x0600099B RID: 2459 RVA: 0x00016406 File Offset: 0x00015406
		public _IExpression _Condition
		{
			get
			{
				if (this.m_expCond == null)
				{
					return new NullExpression();
				}
				return this.m_expCond;
			}
			set
			{
				this.m_expCond = value;
			}
		}

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x0600099C RID: 2460 RVA: 0x0001640F File Offset: 0x0001540F
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x0600099D RID: 2461 RVA: 0x00016417 File Offset: 0x00015417
		// (set) Token: 0x0600099E RID: 2462 RVA: 0x0001642D File Offset: 0x0001542D
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

		// Token: 0x0600099F RID: 2463 RVA: 0x00016436 File Offset: 0x00015436
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060009A0 RID: 2464 RVA: 0x0001643F File Offset: 0x0001543F
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x00016448 File Offset: 0x00015448
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x00016454 File Offset: 0x00015454
		public override _IExprement Duplicate()
		{
			RepeatStatement repeatStatement = new RepeatStatement();
			this.DuplicateCommon(repeatStatement);
			if (this.m_expCond != null)
			{
				repeatStatement.m_expCond = (this.m_expCond.Duplicate() as _IExpression);
			}
			if (this.m_stateControlled != null)
			{
				repeatStatement.m_stateControlled = (this.m_stateControlled.Duplicate() as _IStatement);
			}
			return repeatStatement;
		}

		// Token: 0x04000156 RID: 342
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCond;

		// Token: 0x04000157 RID: 343
		[DefaultSerialization("Controlled")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_stateControlled;
	}
}
