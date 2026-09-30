using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000228 RID: 552
	internal class PragmaOperatorExpression_Green : PragmaExpression_Green, _IPragmaOperatorExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IPragmaOperatorExpression
	{
		// Token: 0x0600243D RID: 9277 RVA: 0x0005C196 File Offset: 0x0005B196
		public PragmaOperatorExpression_Green(PragmaOperator op, _IExpression[] Operands)
		{
			this.m_op = op;
			this.m_Operands = Operands;
		}

		// Token: 0x17000A3F RID: 2623
		// (get) Token: 0x0600243E RID: 9278 RVA: 0x0005C1AC File Offset: 0x0005B1AC
		// (set) Token: 0x0600243F RID: 9279 RVA: 0x0005A471 File Offset: 0x00059471
		public PragmaOperator Code
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

		// Token: 0x17000A40 RID: 2624
		// (get) Token: 0x06002440 RID: 9280 RVA: 0x0005C1B4 File Offset: 0x0005B1B4
		// (set) Token: 0x06002441 RID: 9281 RVA: 0x0005A471 File Offset: 0x00059471
		public Operator Operator
		{
			get
			{
				switch (this.m_op)
				{
				case PragmaOperator.Or:
					return Operator.Or;
				case PragmaOperator.And:
					return Operator.And;
				case PragmaOperator.Not:
					return Operator.Not;
				default:
					return Operator.None;
				}
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A41 RID: 2625
		// (get) Token: 0x06002442 RID: 9282 RVA: 0x0005C1EF File Offset: 0x0005B1EF
		public IList<_IExpression> Operands
		{
			get
			{
				return this.m_Operands;
			}
		}

		// Token: 0x17000A42 RID: 2626
		// (get) Token: 0x06002443 RID: 9283 RVA: 0x0005C1F8 File Offset: 0x0005B1F8
		public IExpression[] AllOperands
		{
			get
			{
				return this.m_Operands;
			}
		}

		// Token: 0x06002444 RID: 9284 RVA: 0x0005A471 File Offset: 0x00059471
		public void AddOperand(_IExpression exp)
		{
			throw new NotSupportedException();
		}

		// Token: 0x06002445 RID: 9285 RVA: 0x00012504 File Offset: 0x00011504
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002446 RID: 9286 RVA: 0x00012516 File Offset: 0x00011516
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006EE RID: 1774
		private readonly _IExpression[] m_Operands;

		// Token: 0x040006EF RID: 1775
		private readonly PragmaOperator m_op;
	}
}
