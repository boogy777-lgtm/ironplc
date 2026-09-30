using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000084 RID: 132
	[TypeGuid("{ba4369e2-ca81-47a1-a76c-cbce4e63b7fc}")]
	[StorageVersion("3.3.0.0")]
	public class CaseStatement : PositionStatement, _ICaseStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICaseStatement
	{
		// Token: 0x06000859 RID: 2137 RVA: 0x0001478C File Offset: 0x0001378C
		public CaseStatement()
		{
		}

		// Token: 0x0600085A RID: 2138 RVA: 0x000147A0 File Offset: 0x000137A0
		internal CaseStatement(_IExpression expSwitch)
		{
			this.m_expSwitch = expSwitch;
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x000147BB File Offset: 0x000137BB
		internal CaseStatement(_IExpression expSwitch, IToken token) : base(token)
		{
			this.m_expSwitch = expSwitch;
		}

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x0600085C RID: 2140 RVA: 0x000147D7 File Offset: 0x000137D7
		public IExpression Switch
		{
			get
			{
				return this._Switch;
			}
		}

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x0600085D RID: 2141 RVA: 0x000147DF File Offset: 0x000137DF
		// (set) Token: 0x0600085E RID: 2142 RVA: 0x000147F5 File Offset: 0x000137F5
		public _IExpression _Switch
		{
			get
			{
				if (this.m_expSwitch == null)
				{
					return new NullExpression();
				}
				return this.m_expSwitch;
			}
			set
			{
				this.m_expSwitch = value;
			}
		}

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x0600085F RID: 2143 RVA: 0x00014800 File Offset: 0x00013800
		public ICase[] Cases
		{
			get
			{
				Case[] array = new Case[this.m_alCases.Count];
				LList<_ICase> alCases = this.m_alCases;
				_ICase[] array2 = array;
				alCases.CopyTo(array2, 0);
				return array;
			}
		}

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x06000860 RID: 2144 RVA: 0x00014830 File Offset: 0x00013830
		public IList<_ICase> _Cases
		{
			get
			{
				return this.m_alCases;
			}
		}

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06000861 RID: 2145 RVA: 0x00014838 File Offset: 0x00013838
		public IStatement Else
		{
			get
			{
				return this._Else;
			}
		}

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000862 RID: 2146 RVA: 0x00014840 File Offset: 0x00013840
		// (set) Token: 0x06000863 RID: 2147 RVA: 0x00014848 File Offset: 0x00013848
		public _IStatement _Else
		{
			get
			{
				return this.m_stateElse;
			}
			set
			{
				this.m_stateElse = value;
			}
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x00014851 File Offset: 0x00013851
		public void AddCase(_ICase case1)
		{
			this.m_alCases.Add(case1);
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x0001485F File Offset: 0x0001385F
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x00014868 File Offset: 0x00013868
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x00014871 File Offset: 0x00013871
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x0001487C File Offset: 0x0001387C
		public override _IExprement Duplicate()
		{
			CaseStatement caseStatement = new CaseStatement();
			this.DuplicateCommon(caseStatement);
			if (this.m_expSwitch != null)
			{
				caseStatement.m_expSwitch = (this.m_expSwitch.Duplicate() as _IExpression);
			}
			foreach (Case @case in this.m_alCases.OfType<Case>())
			{
				caseStatement.AddCase(@case.Duplicate());
			}
			if (this.m_stateElse != null)
			{
				caseStatement.m_stateElse = (this.m_stateElse.Duplicate() as _IStatement);
			}
			return caseStatement;
		}

		// Token: 0x0400011E RID: 286
		[DefaultSerialization("Switch")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expSwitch;

		// Token: 0x0400011F RID: 287
		[DefaultSerialization("Cases")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_ICase> m_alCases = new LList<_ICase>(1);

		// Token: 0x04000120 RID: 288
		[DefaultSerialization("Else")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IStatement m_stateElse;
	}
}
