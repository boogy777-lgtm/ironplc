using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E3 RID: 483
	internal class CaseStatement_Green : Statement_Green, _ICaseStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICaseStatement
	{
		// Token: 0x060021D6 RID: 8662 RVA: 0x0005A745 File Offset: 0x00059745
		internal CaseStatement_Green(_IExpression expSwitch, _ICase[] cases, _IStatement elsecase)
		{
			this.m_expSwitch = expSwitch;
			this.m_cases = cases;
			this.m_stateElse = elsecase;
		}

		// Token: 0x17000953 RID: 2387
		// (get) Token: 0x060021D7 RID: 8663 RVA: 0x0005A762 File Offset: 0x00059762
		public IExpression Switch
		{
			get
			{
				return this._Switch;
			}
		}

		// Token: 0x17000954 RID: 2388
		// (get) Token: 0x060021D8 RID: 8664 RVA: 0x0005A76A File Offset: 0x0005976A
		// (set) Token: 0x060021D9 RID: 8665 RVA: 0x0005A609 File Offset: 0x00059609
		public _IExpression _Switch
		{
			get
			{
				if (this.m_expSwitch == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expSwitch;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x17000955 RID: 2389
		// (get) Token: 0x060021DA RID: 8666 RVA: 0x0005A780 File Offset: 0x00059780
		public ICase[] Cases
		{
			get
			{
				return this.m_cases;
			}
		}

		// Token: 0x17000956 RID: 2390
		// (get) Token: 0x060021DB RID: 8667 RVA: 0x0005A795 File Offset: 0x00059795
		public IList<_ICase> _Cases
		{
			get
			{
				return this.m_cases;
			}
		}

		// Token: 0x17000957 RID: 2391
		// (get) Token: 0x060021DC RID: 8668 RVA: 0x0005A79D File Offset: 0x0005979D
		public IStatement Else
		{
			get
			{
				return this._Else;
			}
		}

		// Token: 0x17000958 RID: 2392
		// (get) Token: 0x060021DD RID: 8669 RVA: 0x0005A7A5 File Offset: 0x000597A5
		// (set) Token: 0x060021DE RID: 8670 RVA: 0x0005A609 File Offset: 0x00059609
		public _IStatement _Else
		{
			get
			{
				return this.m_stateElse;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x060021DF RID: 8671 RVA: 0x0005A609 File Offset: 0x00059609
		[Obsolete("no list access")]
		public void AddCase(_ICase case1)
		{
			throw new NotSupportedException("no manipulation of green tree expressions");
		}

		// Token: 0x060021E0 RID: 8672 RVA: 0x0001485F File Offset: 0x0001385F
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021E1 RID: 8673 RVA: 0x00014871 File Offset: 0x00013871
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0400067D RID: 1661
		private readonly _IExpression m_expSwitch;

		// Token: 0x0400067E RID: 1662
		private readonly _ICase[] m_cases;

		// Token: 0x0400067F RID: 1663
		private readonly _IStatement m_stateElse;
	}
}
