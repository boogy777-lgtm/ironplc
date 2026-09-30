using System;
using System.Collections.Generic;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E7 RID: 487
	internal class SequenceStatement_Green : Statement_Green, _ISequenceStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ISequenceStatement3, ISequenceStatement2, ISequenceStatement
	{
		// Token: 0x060021FD RID: 8701 RVA: 0x0005A8A2 File Offset: 0x000598A2
		public SequenceStatement_Green(_IStatement[] statements)
		{
			this.m_statements = statements;
		}

		// Token: 0x17000965 RID: 2405
		// (get) Token: 0x060021FE RID: 8702 RVA: 0x0005A8B4 File Offset: 0x000598B4
		public IStatement[] Statements
		{
			get
			{
				return this._Statements;
			}
		}

		// Token: 0x17000966 RID: 2406
		// (get) Token: 0x060021FF RID: 8703 RVA: 0x0005A8C9 File Offset: 0x000598C9
		public _IStatement[] _Statements
		{
			get
			{
				return this.m_statements;
			}
		}

		// Token: 0x17000967 RID: 2407
		// (get) Token: 0x06002200 RID: 8704 RVA: 0x0005A8C9 File Offset: 0x000598C9
		public IEnumerable<IStatement> StatementList
		{
			get
			{
				return this.m_statements;
			}
		}

		// Token: 0x17000968 RID: 2408
		// (get) Token: 0x06002201 RID: 8705 RVA: 0x0005A8C9 File Offset: 0x000598C9
		public IList<_IStatement> _StatementList
		{
			get
			{
				return this.m_statements;
			}
		}

		// Token: 0x06002202 RID: 8706 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void AddStatement(IStatement state)
		{
			Debug.Assert(false);
		}

		// Token: 0x06002203 RID: 8707 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void InsertStatement(int i, IStatement state)
		{
			Debug.Assert(false);
		}

		// Token: 0x06002204 RID: 8708 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void RemoveStatement(int i)
		{
			Debug.Assert(false);
		}

		// Token: 0x06002205 RID: 8709 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void Add(_IStatement sm)
		{
			Debug.Assert(false);
		}

		// Token: 0x06002206 RID: 8710 RVA: 0x0005A8D1 File Offset: 0x000598D1
		[Obsolete("no list access")]
		public void Replace(_IStatement sm, int iPosition)
		{
			Debug.Assert(false);
		}

		// Token: 0x17000969 RID: 2409
		public _IStatement this[int i]
		{
			get
			{
				return this.m_statements[i];
			}
			set
			{
				this.m_statements[i] = value;
			}
		}

		// Token: 0x06002209 RID: 8713 RVA: 0x000166B2 File Offset: 0x000156B2
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600220A RID: 8714 RVA: 0x000166C4 File Offset: 0x000156C4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000686 RID: 1670
		private readonly _IStatement[] m_statements;
	}
}
