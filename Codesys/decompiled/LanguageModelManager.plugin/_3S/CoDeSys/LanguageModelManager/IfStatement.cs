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
	// Token: 0x02000091 RID: 145
	[TypeGuid("{e44f126a-f676-4cdc-852e-71e23df8c4b1}")]
	[StorageVersion("3.3.0.0")]
	public class IfStatement : PositionStatement, _IIfStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IIfStatement
	{
		// Token: 0x060008F9 RID: 2297 RVA: 0x000149DB File Offset: 0x000139DB
		public IfStatement()
		{
		}

		// Token: 0x060008FA RID: 2298 RVA: 0x000152C5 File Offset: 0x000142C5
		internal IfStatement(_IExpression expCond)
		{
			this.m_expCond = expCond;
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x000152D4 File Offset: 0x000142D4
		internal IfStatement(_IExpression expCond, IToken token) : base(token)
		{
			this.m_expCond = expCond;
		}

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x060008FC RID: 2300 RVA: 0x000152E4 File Offset: 0x000142E4
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x060008FD RID: 2301 RVA: 0x000152EC File Offset: 0x000142EC
		// (set) Token: 0x060008FE RID: 2302 RVA: 0x00015302 File Offset: 0x00014302
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

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x060008FF RID: 2303 RVA: 0x0001530B File Offset: 0x0001430B
		public IStatement IfThen
		{
			get
			{
				return this._IfThen;
			}
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000900 RID: 2304 RVA: 0x00015313 File Offset: 0x00014313
		// (set) Token: 0x06000901 RID: 2305 RVA: 0x00015329 File Offset: 0x00014329
		public _IStatement _IfThen
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

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000902 RID: 2306 RVA: 0x00015332 File Offset: 0x00014332
		public IStatement IfElse
		{
			get
			{
				return this._IfElse;
			}
		}

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x06000903 RID: 2307 RVA: 0x0001533A File Offset: 0x0001433A
		// (set) Token: 0x06000904 RID: 2308 RVA: 0x00015342 File Offset: 0x00014342
		public _IStatement _IfElse
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

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x06000905 RID: 2309 RVA: 0x0001534C File Offset: 0x0001434C
		public IElseIf[] ElseIf
		{
			get
			{
				if (this.m_alElseIf == null)
				{
					return Array.Empty<IElseIf>();
				}
				_IElseIf[] array = new _IElseIf[this.m_alElseIf.Count];
				this.m_alElseIf.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x06000906 RID: 2310 RVA: 0x00015388 File Offset: 0x00014388
		public IList<_IElseIf> _ElseIf
		{
			get
			{
				if (this.m_alElseIf == null)
				{
					return Array.Empty<_IElseIf>();
				}
				return this.m_alElseIf;
			}
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x0001539E File Offset: 0x0001439E
		public void ClearElseIf()
		{
			if (this.m_alElseIf != null)
			{
				this.m_alElseIf.Clear();
			}
			this.m_alElseIf = null;
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x000153BA File Offset: 0x000143BA
		public void AddElseIf(_IElseIf elseIf)
		{
			if (this.m_alElseIf == null)
			{
				this.m_alElseIf = new LList<_IElseIf>(1);
			}
			this.m_alElseIf.Add(elseIf);
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x000153DC File Offset: 0x000143DC
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x000153E5 File Offset: 0x000143E5
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x000153EE File Offset: 0x000143EE
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x000153F8 File Offset: 0x000143F8
		public override _IExprement Duplicate()
		{
			IfStatement ifStatement = new IfStatement();
			this.DuplicateCommon(ifStatement);
			if (this.m_expCond != null)
			{
				ifStatement.m_expCond = (this.m_expCond.Duplicate() as _IExpression);
			}
			if (this.m_smIfThen != null)
			{
				ifStatement.m_smIfThen = (this.m_smIfThen.Duplicate() as _IStatement);
			}
			if (this.m_smIfElse != null)
			{
				ifStatement.m_smIfElse = (this.m_smIfElse.Duplicate() as _IStatement);
			}
			if (this.m_alElseIf != null)
			{
				ifStatement.m_alElseIf = new LList<_IElseIf>(this.m_alElseIf.Count);
				foreach (_IElseIf ielseIf in this.m_alElseIf)
				{
					ifStatement.AddElseIf(ielseIf.Duplicate() as _IElseIf);
				}
				ifStatement.m_alElseIf.TrimExcess();
			}
			return ifStatement;
		}

		// Token: 0x04000137 RID: 311
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCond;

		// Token: 0x04000138 RID: 312
		[DefaultSerialization("Then")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_smIfThen;

		// Token: 0x04000139 RID: 313
		[DefaultSerialization("Else")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_smIfElse;

		// Token: 0x0400013A RID: 314
		[DefaultSerialization("ElseIf")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IElseIf> m_alElseIf;
	}
}
