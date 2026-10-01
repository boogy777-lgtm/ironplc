using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000A1 RID: 161
	[TypeGuid("{4e534dfd-b819-4d1d-b488-5f909b454ffa}")]
	[StorageVersion("3.3.0.0")]
	public class SequenceStatement : Statement, _ISequenceStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ISequenceStatement3, ISequenceStatement2, ISequenceStatement, IPositionExprement, ILengthExprement
	{
		// Token: 0x060009AC RID: 2476 RVA: 0x00016519 File Offset: 0x00015519
		public SequenceStatement()
		{
			this.m_alStatements = new LList<_IStatement>(1);
		}

		// Token: 0x060009AD RID: 2477 RVA: 0x0001652D File Offset: 0x0001552D
		public SequenceStatement(IToken token) : base(token)
		{
			this.m_alStatements = new LList<_IStatement>(1);
		}

		// Token: 0x060009AE RID: 2478 RVA: 0x00016542 File Offset: 0x00015542
		public SequenceStatement(int nNumStatements)
		{
			this.m_alStatements = new LList<_IStatement>(nNumStatements);
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x060009AF RID: 2479 RVA: 0x00016558 File Offset: 0x00015558
		public IStatement[] Statements
		{
			get
			{
				Statement[] array = new Statement[this.m_alStatements.Count];
				LList<_IStatement> alStatements = this.m_alStatements;
				_IStatement[] array2 = array;
				alStatements.CopyTo(array2, 0);
				return array;
			}
		}

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x060009B0 RID: 2480 RVA: 0x00016588 File Offset: 0x00015588
		public IEnumerable<IStatement> StatementList
		{
			get
			{
				return this.m_alStatements;
			}
		}

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x060009B1 RID: 2481 RVA: 0x00016588 File Offset: 0x00015588
		public IList<_IStatement> _StatementList
		{
			get
			{
				return this.m_alStatements;
			}
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x00016590 File Offset: 0x00015590
		public void AddStatement(IStatement state)
		{
			this.Add(state as Statement);
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x000165A0 File Offset: 0x000155A0
		public void InsertStatement(int i, IStatement state)
		{
			if (i < 0 || i > this.m_alStatements.Count)
			{
				throw new ArgumentOutOfRangeException("i");
			}
			SequenceStatement sequenceStatement = state as SequenceStatement;
			if (sequenceStatement != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35620 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35640)
			{
				this.m_alStatements.InsertRange(i, sequenceStatement._StatementList);
				return;
			}
			this.m_alStatements.Insert(i, state as Statement);
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x0001661B File Offset: 0x0001561B
		public void RemoveStatement(int i)
		{
			if (i < 0 || i >= this.m_alStatements.Count)
			{
				throw new ArgumentOutOfRangeException("i");
			}
			this.m_alStatements.RemoveAt(i);
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x00016648 File Offset: 0x00015648
		public void Add(_IStatement sm)
		{
			if (sm == null)
			{
				return;
			}
			SequenceStatement sequenceStatement = sm as SequenceStatement;
			if (sequenceStatement != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35620 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35640)
			{
				this.m_alStatements.AddRange(sequenceStatement._StatementList);
				return;
			}
			this.m_alStatements.Add(sm);
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x000166A3 File Offset: 0x000156A3
		public void Replace(_IStatement sm, int iPosition)
		{
			this.m_alStatements[iPosition] = sm;
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x000166B2 File Offset: 0x000156B2
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060009B8 RID: 2488 RVA: 0x000166BB File Offset: 0x000156BB
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060009B9 RID: 2489 RVA: 0x000166C4 File Offset: 0x000156C4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x000166D0 File Offset: 0x000156D0
		public override _IExprement Duplicate()
		{
			SequenceStatement sequenceStatement = new SequenceStatement(this.m_alStatements.Count);
			this.DuplicateCommon(sequenceStatement);
			foreach (_IStatement istatement in this.m_alStatements)
			{
				sequenceStatement.Add(istatement.Duplicate() as _IStatement);
			}
			return sequenceStatement;
		}

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x060009BB RID: 2491 RVA: 0x00016740 File Offset: 0x00015740
		// (set) Token: 0x060009BC RID: 2492 RVA: 0x00016748 File Offset: 0x00015748
		[Obfuscation(Feature = "rename")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this.m_position;
			}
			set
			{
				this.m_position = value;
			}
		}

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x060009BD RID: 2493 RVA: 0x00016751 File Offset: 0x00015751
		// (set) Token: 0x060009BE RID: 2494 RVA: 0x00016759 File Offset: 0x00015759
		[DefaultSerialization("Length")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public override short LengthIntern
		{
			get
			{
				return this.m_sLength;
			}
			set
			{
				this.m_sLength = value;
			}
		}

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x060009BF RID: 2495 RVA: 0x00016764 File Offset: 0x00015764
		// (set) Token: 0x060009C0 RID: 2496 RVA: 0x000167C4 File Offset: 0x000157C4
		[DefaultSerialization("Statements")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private ArrayList StatesToSave
		{
			get
			{
				ArrayList arrayList = new ArrayList(this.m_alStatements.Count);
				foreach (_IStatement value in this.m_alStatements)
				{
					arrayList.Add(value);
				}
				return arrayList;
			}
			set
			{
				if (value == null || value.Count == 0)
				{
					return;
				}
				foreach (object obj in value)
				{
					Statement statement = (Statement)obj;
					this.m_alStatements.Add(statement);
				}
			}
		}

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x060009C1 RID: 2497 RVA: 0x00005F0F File Offset: 0x00004F0F
		// (set) Token: 0x060009C2 RID: 2498 RVA: 0x0001682C File Offset: 0x0001582C
		[DefaultSerialization("StatementsNew")]
		[StorageVersion("3.3.2.0")]
		[StorageSaveAsNonGenericCollection("3.3.2.0-3.5.0.255")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Major Code Smell", "S1168:Empty arrays and collections should be returned instead of null", Justification = "Behaviour cannot be changed because of released interfaces")]
		private List<Statement> Dummy
		{
			get
			{
				return null;
			}
			set
			{
				if (value != null)
				{
					this.m_alStatements.AddRange(value);
				}
			}
		}

		// Token: 0x04000159 RID: 345
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_position;

		// Token: 0x0400015A RID: 346
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x0400015B RID: 347
		private readonly LList<_IStatement> m_alStatements;
	}
}
