using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F6 RID: 502
	internal class AssignmentExpression_Green : Expression_Green, _IAssignmentExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IAssignmentExpression
	{
		// Token: 0x06002270 RID: 8816 RVA: 0x0005ACC6 File Offset: 0x00059CC6
		public AssignmentExpression_Green(_IExpression expLValue, _IExpression expRValue, Operator kindof)
		{
			this.m_expLValue = expLValue;
			this.m_expRValue = expRValue;
			this.m_op = kindof;
		}

		// Token: 0x17000991 RID: 2449
		// (get) Token: 0x06002271 RID: 8817 RVA: 0x0005ACE3 File Offset: 0x00059CE3
		// (set) Token: 0x06002272 RID: 8818 RVA: 0x0005A471 File Offset: 0x00059471
		public Operator KindOf
		{
			get
			{
				return this.m_op;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000992 RID: 2450
		// (get) Token: 0x06002273 RID: 8819 RVA: 0x0005ACEB File Offset: 0x00059CEB
		public IExpression LValue
		{
			get
			{
				return this._LValue;
			}
		}

		// Token: 0x17000993 RID: 2451
		// (get) Token: 0x06002274 RID: 8820 RVA: 0x0005ACF3 File Offset: 0x00059CF3
		// (set) Token: 0x06002275 RID: 8821 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _LValue
		{
			get
			{
				if (this.m_expLValue != null)
				{
					return this.m_expLValue;
				}
				return new NullExpression_Green();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000994 RID: 2452
		// (get) Token: 0x06002276 RID: 8822 RVA: 0x0005AD09 File Offset: 0x00059D09
		public IExpression RValue
		{
			get
			{
				return this._RValue;
			}
		}

		// Token: 0x17000995 RID: 2453
		// (get) Token: 0x06002277 RID: 8823 RVA: 0x0005AD11 File Offset: 0x00059D11
		// (set) Token: 0x06002278 RID: 8824 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _RValue
		{
			get
			{
				if (this.m_expRValue == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expRValue;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002279 RID: 8825 RVA: 0x0000B86B File Offset: 0x0000A86B
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600227A RID: 8826 RVA: 0x0000B87D File Offset: 0x0000A87D
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0400069E RID: 1694
		private readonly _IExpression m_expLValue;

		// Token: 0x0400069F RID: 1695
		private readonly _IExpression m_expRValue;

		// Token: 0x040006A0 RID: 1696
		private readonly Operator m_op;
	}
}
