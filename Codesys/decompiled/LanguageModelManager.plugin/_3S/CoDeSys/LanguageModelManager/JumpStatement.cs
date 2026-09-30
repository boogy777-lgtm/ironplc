using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000093 RID: 147
	[TypeGuid("{becafce5-82a0-4a8c-a117-0ac033bf694d}")]
	[StorageVersion("3.3.0.0")]
	public class JumpStatement : PositionStatement, _IJumpStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IJumpStatement
	{
		// Token: 0x06000912 RID: 2322 RVA: 0x0001554B File Offset: 0x0001454B
		public JumpStatement()
		{
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x0001555E File Offset: 0x0001455E
		public JumpStatement(string stLabel)
		{
			this.m_stLabel = stLabel;
		}

		// Token: 0x06000914 RID: 2324 RVA: 0x00015578 File Offset: 0x00014578
		public JumpStatement(string stLabel, IToken token) : base(token)
		{
			this.m_stLabel = stLabel;
		}

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06000915 RID: 2325 RVA: 0x00015593 File Offset: 0x00014593
		// (set) Token: 0x06000916 RID: 2326 RVA: 0x000155B8 File Offset: 0x000145B8
		public string Label
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3211)
				{
					return this.m_stLabel.ToUpperInvariant();
				}
				return this.m_stLabel;
			}
			set
			{
				this.m_stLabel = value;
			}
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x000155C1 File Offset: 0x000145C1
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x000155CA File Offset: 0x000145CA
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x000155D3 File Offset: 0x000145D3
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x0600091A RID: 2330 RVA: 0x000155DC File Offset: 0x000145DC
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x0600091B RID: 2331 RVA: 0x000155E4 File Offset: 0x000145E4
		// (set) Token: 0x0600091C RID: 2332 RVA: 0x000155EC File Offset: 0x000145EC
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

		// Token: 0x0600091D RID: 2333 RVA: 0x000155F8 File Offset: 0x000145F8
		public override _IExprement Duplicate()
		{
			JumpStatement jumpStatement = new JumpStatement();
			this.DuplicateCommon(jumpStatement);
			jumpStatement.m_stLabel = this.m_stLabel;
			if (this._Condition != null)
			{
				jumpStatement._Condition = (this._Condition.Duplicate() as _IExpression);
			}
			return jumpStatement;
		}

		// Token: 0x0400013C RID: 316
		[DefaultSerialization("Label")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stLabel = string.Empty;

		// Token: 0x0400013D RID: 317
		[DefaultSerialization("Condition")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_expCondition;
	}
}
