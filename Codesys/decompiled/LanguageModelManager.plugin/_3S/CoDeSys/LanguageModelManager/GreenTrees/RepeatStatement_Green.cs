using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001DF RID: 479
	internal class RepeatStatement_Green : Statement_Green, _IRepeatStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IRepeatStatement
	{
		// Token: 0x060021B5 RID: 8629 RVA: 0x0005A633 File Offset: 0x00059633
		internal RepeatStatement_Green(_IExpression expCond, _IStatement stateControlled)
		{
			this.m_expCond = expCond;
			this.m_stateControlled = stateControlled;
		}

		// Token: 0x17000944 RID: 2372
		// (get) Token: 0x060021B6 RID: 8630 RVA: 0x0005A649 File Offset: 0x00059649
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000945 RID: 2373
		// (get) Token: 0x060021B7 RID: 8631 RVA: 0x0005A651 File Offset: 0x00059651
		// (set) Token: 0x060021B8 RID: 8632 RVA: 0x0005A609 File Offset: 0x00059609
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

		// Token: 0x17000946 RID: 2374
		// (get) Token: 0x060021B9 RID: 8633 RVA: 0x0005A667 File Offset: 0x00059667
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x17000947 RID: 2375
		// (get) Token: 0x060021BA RID: 8634 RVA: 0x0005A66F File Offset: 0x0005966F
		// (set) Token: 0x060021BB RID: 8635 RVA: 0x0005A609 File Offset: 0x00059609
		public _IStatement _Controlled
		{
			get
			{
				if (this.m_stateControlled == null)
				{
					return new NullStatement_Green();
				}
				return this.m_stateControlled;
			}
			set
			{
				throw new NotSupportedException("no manipulation of green tree expressions");
			}
		}

		// Token: 0x060021BC RID: 8636 RVA: 0x00016436 File Offset: 0x00015436
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021BD RID: 8637 RVA: 0x00016448 File Offset: 0x00015448
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000676 RID: 1654
		private readonly _IExpression m_expCond;

		// Token: 0x04000677 RID: 1655
		private readonly _IStatement m_stateControlled;
	}
}
