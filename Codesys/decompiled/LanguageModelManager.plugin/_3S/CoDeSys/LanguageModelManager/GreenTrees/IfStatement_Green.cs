using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E9 RID: 489
	internal class IfStatement_Green : Statement_Green, _IIfStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IIfStatement
	{
		// Token: 0x06002214 RID: 8724 RVA: 0x0005A940 File Offset: 0x00059940
		internal IfStatement_Green(_IExpression expCond, _IStatement stateThen, _IStatement stateElse, _IElseIf[] elsifs)
		{
			this.m_expCond = expCond;
			this.m_smIfThen = stateThen;
			this.m_smIfElse = stateElse;
			if (elsifs == null || elsifs.Length == 0)
			{
				this.m_elseIfs = null;
				return;
			}
			this.m_elseIfs = elsifs;
		}

		// Token: 0x1700096E RID: 2414
		// (get) Token: 0x06002215 RID: 8725 RVA: 0x0005A976 File Offset: 0x00059976
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x1700096F RID: 2415
		// (get) Token: 0x06002216 RID: 8726 RVA: 0x0005A97E File Offset: 0x0005997E
		// (set) Token: 0x06002217 RID: 8727 RVA: 0x0005A609 File Offset: 0x00059609
		public _IExpression _Condition
		{
			get
			{
				if (this.m_expCond == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expCond;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x17000970 RID: 2416
		// (get) Token: 0x06002218 RID: 8728 RVA: 0x0005A994 File Offset: 0x00059994
		public IStatement IfThen
		{
			get
			{
				return this._IfThen;
			}
		}

		// Token: 0x17000971 RID: 2417
		// (get) Token: 0x06002219 RID: 8729 RVA: 0x0005A99C File Offset: 0x0005999C
		// (set) Token: 0x0600221A RID: 8730 RVA: 0x0005A609 File Offset: 0x00059609
		public _IStatement _IfThen
		{
			get
			{
				if (this.m_smIfThen == null)
				{
					return new NullStatement_Green();
				}
				return this.m_smIfThen;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x17000972 RID: 2418
		// (get) Token: 0x0600221B RID: 8731 RVA: 0x0005A9B2 File Offset: 0x000599B2
		public IStatement IfElse
		{
			get
			{
				return this._IfElse;
			}
		}

		// Token: 0x17000973 RID: 2419
		// (get) Token: 0x0600221C RID: 8732 RVA: 0x0005A9BA File Offset: 0x000599BA
		// (set) Token: 0x0600221D RID: 8733 RVA: 0x0005A609 File Offset: 0x00059609
		public _IStatement _IfElse
		{
			get
			{
				return this.m_smIfElse;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x17000974 RID: 2420
		// (get) Token: 0x0600221E RID: 8734 RVA: 0x0005A9C4 File Offset: 0x000599C4
		public IElseIf[] ElseIf
		{
			get
			{
				if (this.m_elseIfs == null)
				{
					return Array.Empty<_IElseIf>();
				}
				return this.m_elseIfs;
			}
		}

		// Token: 0x17000975 RID: 2421
		// (get) Token: 0x0600221F RID: 8735 RVA: 0x0005A9E9 File Offset: 0x000599E9
		public IList<_IElseIf> _ElseIf
		{
			get
			{
				if (this.m_elseIfs == null)
				{
					return Array.Empty<_IElseIf>();
				}
				return this.m_elseIfs;
			}
		}

		// Token: 0x06002220 RID: 8736 RVA: 0x0005A9FF File Offset: 0x000599FF
		public void ClearElseIf()
		{
			this.m_elseIfs = null;
		}

		// Token: 0x06002221 RID: 8737 RVA: 0x0005A609 File Offset: 0x00059609
		[Obsolete("no list access")]
		public void AddElseIf(_IElseIf elseIf)
		{
			throw new NotSupportedException("no manipulation of green tree expressions");
		}

		// Token: 0x06002222 RID: 8738 RVA: 0x000153DC File Offset: 0x000143DC
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002223 RID: 8739 RVA: 0x000153EE File Offset: 0x000143EE
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000689 RID: 1673
		private readonly _IExpression m_expCond;

		// Token: 0x0400068A RID: 1674
		private readonly _IStatement m_smIfThen;

		// Token: 0x0400068B RID: 1675
		private readonly _IStatement m_smIfElse;

		// Token: 0x0400068C RID: 1676
		private _IElseIf[] m_elseIfs;
	}
}
