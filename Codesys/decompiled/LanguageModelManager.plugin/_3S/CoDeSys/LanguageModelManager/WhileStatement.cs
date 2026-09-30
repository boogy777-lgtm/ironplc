using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A9 RID: 169
	[TypeGuid("{95226790-629a-43e8-9fe7-916c08c3576f}")]
	[StorageVersion("3.3.0.0")]
	public class WhileStatement : PositionStatement, _IWhileStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IWhileStatement
	{
		// Token: 0x06000A2E RID: 2606 RVA: 0x000149DB File Offset: 0x000139DB
		public WhileStatement()
		{
		}

		// Token: 0x06000A2F RID: 2607 RVA: 0x00017247 File Offset: 0x00016247
		internal WhileStatement(_IExpression expCond, _IStatement stateControlled)
		{
			this.m_expCond = expCond;
			this.m_stateControlled = stateControlled;
		}

		// Token: 0x06000A30 RID: 2608 RVA: 0x0001725D File Offset: 0x0001625D
		internal WhileStatement(_IExpression expCond, _IStatement stateControlled, IToken token) : base(token)
		{
			this.m_expCond = expCond;
			this.m_stateControlled = stateControlled;
		}

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000A31 RID: 2609 RVA: 0x00017274 File Offset: 0x00016274
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000A32 RID: 2610 RVA: 0x0001727C File Offset: 0x0001627C
		// (set) Token: 0x06000A33 RID: 2611 RVA: 0x00017292 File Offset: 0x00016292
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

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000A34 RID: 2612 RVA: 0x0001729B File Offset: 0x0001629B
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06000A35 RID: 2613 RVA: 0x000172A3 File Offset: 0x000162A3
		// (set) Token: 0x06000A36 RID: 2614 RVA: 0x000172B9 File Offset: 0x000162B9
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

		// Token: 0x06000A37 RID: 2615 RVA: 0x000172C2 File Offset: 0x000162C2
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000A38 RID: 2616 RVA: 0x000172CB File Offset: 0x000162CB
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000A39 RID: 2617 RVA: 0x000172D4 File Offset: 0x000162D4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000A3A RID: 2618 RVA: 0x000172E0 File Offset: 0x000162E0
		public override _IExprement Duplicate()
		{
			WhileStatement whileStatement = new WhileStatement();
			this.DuplicateCommon(whileStatement);
			if (this.m_expCond != null)
			{
				whileStatement.m_expCond = (this.m_expCond.Duplicate() as _IExpression);
			}
			if (this.m_stateControlled != null)
			{
				whileStatement.m_stateControlled = (this.m_stateControlled.Duplicate() as _IStatement);
			}
			return whileStatement;
		}

		// Token: 0x04000179 RID: 377
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCond;

		// Token: 0x0400017A RID: 378
		[DefaultSerialization("Controlled")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_stateControlled;
	}
}
