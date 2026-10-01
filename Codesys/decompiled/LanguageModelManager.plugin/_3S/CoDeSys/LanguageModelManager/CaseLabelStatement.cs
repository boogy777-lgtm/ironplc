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
	// Token: 0x02000083 RID: 131
	[TypeGuid("{8f2c0a58-3eca-4f44-8748-94e16974a1a1}")]
	[StorageVersion("3.3.0.0")]
	public class CaseLabelStatement : PositionStatement, _ICaseLabelStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICaseLabelStatement
	{
		// Token: 0x0600084B RID: 2123 RVA: 0x00014610 File Offset: 0x00013610
		public CaseLabelStatement()
		{
		}

		// Token: 0x0600084C RID: 2124 RVA: 0x00014624 File Offset: 0x00013624
		internal CaseLabelStatement(IToken token) : base(token)
		{
		}

		// Token: 0x0600084D RID: 2125 RVA: 0x00014639 File Offset: 0x00013639
		internal CaseLabelStatement(_IExpression exp)
		{
			this.m_alExpcases.Add(exp);
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x00014659 File Offset: 0x00013659
		internal CaseLabelStatement(_IExpression exp, IToken token) : base(token)
		{
			this.m_alExpcases.Add(exp);
		}

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x0600084F RID: 2127 RVA: 0x0001467C File Offset: 0x0001367C
		public IExpression[] cases
		{
			get
			{
				_IExpression[] array = new _IExpression[this.m_alExpcases.Count];
				this.m_alExpcases.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000850 RID: 2128 RVA: 0x000146AA File Offset: 0x000136AA
		public IList<_IExpression> _cases
		{
			get
			{
				return this.m_alExpcases;
			}
		}

		// Token: 0x170001DB RID: 475
		public _IExpression this[int i]
		{
			get
			{
				return this.m_alExpcases[i] as Expression;
			}
			set
			{
				this.m_alExpcases[i] = value;
			}
		}

		// Token: 0x06000853 RID: 2131 RVA: 0x000146D4 File Offset: 0x000136D4
		public void AddCase(_IExpression expCase)
		{
			this.m_alExpcases.Add(expCase);
		}

		// Token: 0x06000854 RID: 2132 RVA: 0x000146E2 File Offset: 0x000136E2
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000855 RID: 2133 RVA: 0x000146EB File Offset: 0x000136EB
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000856 RID: 2134 RVA: 0x000146F4 File Offset: 0x000136F4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x000146FD File Offset: 0x000136FD
		public override ISourcePosition GetPosition()
		{
			if (this._cases.Count == 0)
			{
				return SourcePosition.Empty;
			}
			return this.m_alExpcases[0].GetPosition();
		}

		// Token: 0x06000858 RID: 2136 RVA: 0x00014724 File Offset: 0x00013724
		public override _IExprement Duplicate()
		{
			CaseLabelStatement caseLabelStatement = new CaseLabelStatement();
			this.DuplicateCommon(caseLabelStatement);
			foreach (_IExpression iexpression in this.m_alExpcases)
			{
				caseLabelStatement.AddCase(iexpression.Duplicate() as Expression);
			}
			return caseLabelStatement;
		}

		// Token: 0x0400011D RID: 285
		[DefaultSerialization("Cases")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alExpcases = new LList<_IExpression>(1);
	}
}
