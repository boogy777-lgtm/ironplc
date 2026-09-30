using System;
using System.Collections.Generic;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200009D RID: 157
	[TypeGuid("{9ee89bb4-ba5d-46b6-8340-4e88a7b1689a}")]
	[StorageVersion("3.3.0.0")]
	public class PragmaIfStatement : PositionStatement, _IPragmaIfStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaIfStatement
	{
		// Token: 0x0600097B RID: 2427 RVA: 0x000149DB File Offset: 0x000139DB
		public PragmaIfStatement()
		{
		}

		// Token: 0x0600097C RID: 2428 RVA: 0x00016124 File Offset: 0x00015124
		internal PragmaIfStatement(_IExpression expCond, IToken token) : base(token)
		{
			this.m_expCond = expCond;
		}

		// Token: 0x0600097D RID: 2429 RVA: 0x00016134 File Offset: 0x00015134
		internal PragmaIfStatement(_IExpression expCond)
		{
			this.m_expCond = expCond;
		}

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x0600097E RID: 2430 RVA: 0x00016143 File Offset: 0x00015143
		// (set) Token: 0x0600097F RID: 2431 RVA: 0x00016159 File Offset: 0x00015159
		public _IExpression Condition
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

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x06000980 RID: 2432 RVA: 0x00016162 File Offset: 0x00015162
		// (set) Token: 0x06000981 RID: 2433 RVA: 0x00016178 File Offset: 0x00015178
		public _IStatement IfThen
		{
			get
			{
				if (this.m_smIfThen == null)
				{
					return new NullStatement();
				}
				return this.m_smIfThen;
			}
			set
			{
				this.m_smIfThen = value;
			}
		}

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x06000982 RID: 2434 RVA: 0x00016181 File Offset: 0x00015181
		// (set) Token: 0x06000983 RID: 2435 RVA: 0x00016189 File Offset: 0x00015189
		public _IStatement IfElse
		{
			get
			{
				return this.m_smIfElse;
			}
			set
			{
				this.m_smIfElse = value;
			}
		}

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x06000984 RID: 2436 RVA: 0x00016192 File Offset: 0x00015192
		public IList<_IPragmaElseIf> ElseIf
		{
			get
			{
				if (this.m_alElseIf == null)
				{
					return Array.Empty<PragmaElseIf>();
				}
				return this.m_alElseIf;
			}
		}

		// Token: 0x06000985 RID: 2437 RVA: 0x000161A8 File Offset: 0x000151A8
		public void AddElseIf(_IPragmaElseIf elseIf)
		{
			if (this.m_alElseIf == null)
			{
				this.m_alElseIf = new LList<_IPragmaElseIf>(1);
			}
			this.m_alElseIf.Add(elseIf);
		}

		// Token: 0x06000986 RID: 2438 RVA: 0x000161CA File Offset: 0x000151CA
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x000161D3 File Offset: 0x000151D3
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x000161DC File Offset: 0x000151DC
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x000161E8 File Offset: 0x000151E8
		public override _IExprement Duplicate()
		{
			PragmaIfStatement pragmaIfStatement = new PragmaIfStatement();
			this.DuplicateCommon(pragmaIfStatement);
			if (this.m_expCond != null)
			{
				pragmaIfStatement.m_expCond = (this.m_expCond.Duplicate() as Expression);
			}
			if (this.m_smIfThen != null)
			{
				pragmaIfStatement.m_smIfThen = (this.m_smIfThen.Duplicate() as Statement);
			}
			if (this.m_smIfElse != null)
			{
				pragmaIfStatement.m_smIfElse = (this.m_smIfElse.Duplicate() as Statement);
			}
			if (this.m_alElseIf != null)
			{
				pragmaIfStatement.m_alElseIf = new LList<_IPragmaElseIf>(this.m_alElseIf.Count);
				foreach (_IPragmaElseIf ipragmaElseIf in this.m_alElseIf)
				{
					pragmaIfStatement.AddElseIf(ipragmaElseIf.Duplicate());
				}
				pragmaIfStatement.m_alElseIf.TrimExcess();
			}
			return pragmaIfStatement;
		}

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x0600098A RID: 2442 RVA: 0x000162CC File Offset: 0x000152CC
		public IElseIf[] ElseIfs
		{
			get
			{
				if (this.m_alElseIf == null)
				{
					return Array.Empty<IElseIf>();
				}
				IElseIf[] array = new IElseIf[this.m_alElseIf.Count];
				for (int i = 0; i < this.m_alElseIf.Count; i++)
				{
					array[i] = (this.m_alElseIf[i] as PragmaElseIf).CreateElseIf();
				}
				return array;
			}
		}

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x0600098B RID: 2443 RVA: 0x00016328 File Offset: 0x00015328
		public IStatement IfElseStatement
		{
			get
			{
				return this.IfElse;
			}
		}

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x0600098C RID: 2444 RVA: 0x00016330 File Offset: 0x00015330
		public IExpression ConditionExpression
		{
			get
			{
				return this.Condition;
			}
		}

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x0600098D RID: 2445 RVA: 0x00016338 File Offset: 0x00015338
		public IStatement IfThenStatement
		{
			get
			{
				return this.IfThen;
			}
		}

		// Token: 0x04000151 RID: 337
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCond;

		// Token: 0x04000152 RID: 338
		[DefaultSerialization("Then")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_smIfThen;

		// Token: 0x04000153 RID: 339
		[DefaultSerialization("Else")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_smIfElse;

		// Token: 0x04000154 RID: 340
		[DefaultSerialization("ElseIf")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IPragmaElseIf> m_alElseIf;
	}
}
