using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000081 RID: 129
	[TypeGuid("{24b9ce54-a44a-40e4-94cb-ae4feb61a4d0}")]
	[StorageVersion("3.3.0.0")]
	public class BreakPointStatement : PositionStatement, _IBreakPointStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IBreakPointStatement
	{
		// Token: 0x0600083A RID: 2106 RVA: 0x000144CC File Offset: 0x000134CC
		public BreakPointStatement()
		{
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x000144EA File Offset: 0x000134EA
		internal BreakPointStatement(IToken token, long lPosition, long lSuccessorPosition) : base(token)
		{
			this.m_lPosition = lPosition;
			this.m_lSuccessorPosition = lSuccessorPosition;
		}

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x0600083C RID: 2108 RVA: 0x00014517 File Offset: 0x00013517
		// (set) Token: 0x0600083D RID: 2109 RVA: 0x0001451F File Offset: 0x0001351F
		public long BPPosition
		{
			get
			{
				return this.m_lPosition;
			}
			set
			{
				this.m_lPosition = value;
			}
		}

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x0600083E RID: 2110 RVA: 0x00014528 File Offset: 0x00013528
		// (set) Token: 0x0600083F RID: 2111 RVA: 0x00014530 File Offset: 0x00013530
		public long SuccessorPosition
		{
			get
			{
				return this.m_lSuccessorPosition;
			}
			set
			{
				this.m_lSuccessorPosition = value;
			}
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x00014539 File Offset: 0x00013539
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x00014542 File Offset: 0x00013542
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0001454B File Offset: 0x0001354B
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x00014554 File Offset: 0x00013554
		public override _IExprement Duplicate()
		{
			BreakPointStatement breakPointStatement = new BreakPointStatement();
			this.DuplicateCommon(breakPointStatement);
			breakPointStatement.m_lPosition = this.m_lPosition;
			breakPointStatement.m_lSuccessorPosition = this.m_lSuccessorPosition;
			return breakPointStatement;
		}

		// Token: 0x04000119 RID: 281
		[DefaultSerialization("Position")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private long m_lPosition = Exprement.InvalidPosition;

		// Token: 0x0400011A RID: 282
		[DefaultSerialization("SuccessorPosition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private long m_lSuccessorPosition = Exprement.InvalidPosition;
	}
}
