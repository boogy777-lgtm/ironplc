using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001E4 RID: 484
	internal class ForStatement_Green : Statement_Green, _IForStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IForStatement
	{
		// Token: 0x060021E2 RID: 8674 RVA: 0x0005A7AD File Offset: 0x000597AD
		internal ForStatement_Green(_IExpression assCounterStart, _IExpression expCounter, _IExpression expBy, _IExpression expUpper, _IExpression expCondition, _IStatement stateControlled)
		{
			this.m_assCounterStart = assCounterStart;
			this.m_expUpper = expUpper;
			this._By = expBy;
			this.m_stateControlled = stateControlled;
			this._Counter = expCounter;
			this._Condition = expCondition;
		}

		// Token: 0x17000959 RID: 2393
		// (get) Token: 0x060021E3 RID: 8675 RVA: 0x0005A7E2 File Offset: 0x000597E2
		public IExpression CounterStart
		{
			get
			{
				return this._CounterStart;
			}
		}

		// Token: 0x1700095A RID: 2394
		// (get) Token: 0x060021E4 RID: 8676 RVA: 0x0005A7EA File Offset: 0x000597EA
		// (set) Token: 0x060021E5 RID: 8677 RVA: 0x0005A800 File Offset: 0x00059800
		public _IExpression _CounterStart
		{
			get
			{
				if (this.m_assCounterStart == null)
				{
					return new NullExpression_Green();
				}
				return this.m_assCounterStart;
			}
			set
			{
				this.m_assCounterStart = value;
			}
		}

		// Token: 0x1700095B RID: 2395
		// (get) Token: 0x060021E6 RID: 8678 RVA: 0x0005A809 File Offset: 0x00059809
		public IExpression UpperBound
		{
			get
			{
				return this._UpperBound;
			}
		}

		// Token: 0x1700095C RID: 2396
		// (get) Token: 0x060021E7 RID: 8679 RVA: 0x0005A811 File Offset: 0x00059811
		// (set) Token: 0x060021E8 RID: 8680 RVA: 0x0005A827 File Offset: 0x00059827
		public _IExpression _UpperBound
		{
			get
			{
				if (this.m_expUpper != null)
				{
					return this.m_expUpper;
				}
				return new NullExpression_Green();
			}
			set
			{
				this.m_expUpper = value;
			}
		}

		// Token: 0x1700095D RID: 2397
		// (get) Token: 0x060021E9 RID: 8681 RVA: 0x0005A830 File Offset: 0x00059830
		public IExpression By
		{
			get
			{
				return this._By;
			}
		}

		// Token: 0x1700095E RID: 2398
		// (get) Token: 0x060021EA RID: 8682 RVA: 0x0005A838 File Offset: 0x00059838
		// (set) Token: 0x060021EB RID: 8683 RVA: 0x0005A840 File Offset: 0x00059840
		public _IExpression _By { get; set; }

		// Token: 0x1700095F RID: 2399
		// (get) Token: 0x060021EC RID: 8684 RVA: 0x0005A849 File Offset: 0x00059849
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000960 RID: 2400
		// (get) Token: 0x060021ED RID: 8685 RVA: 0x0005A851 File Offset: 0x00059851
		// (set) Token: 0x060021EE RID: 8686 RVA: 0x0005A859 File Offset: 0x00059859
		public _IExpression _Condition { get; set; }

		// Token: 0x17000961 RID: 2401
		// (get) Token: 0x060021EF RID: 8687 RVA: 0x0005A862 File Offset: 0x00059862
		public IExpression Counter
		{
			get
			{
				return this._Counter;
			}
		}

		// Token: 0x17000962 RID: 2402
		// (get) Token: 0x060021F0 RID: 8688 RVA: 0x0005A86A File Offset: 0x0005986A
		// (set) Token: 0x060021F1 RID: 8689 RVA: 0x0005A872 File Offset: 0x00059872
		public _IExpression _Counter { get; set; }

		// Token: 0x17000963 RID: 2403
		// (get) Token: 0x060021F2 RID: 8690 RVA: 0x0005A87B File Offset: 0x0005987B
		public IStatement Controlled
		{
			get
			{
				return this._Controlled;
			}
		}

		// Token: 0x17000964 RID: 2404
		// (get) Token: 0x060021F3 RID: 8691 RVA: 0x0005A883 File Offset: 0x00059883
		// (set) Token: 0x060021F4 RID: 8692 RVA: 0x0005A899 File Offset: 0x00059899
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
				this.m_stateControlled = value;
			}
		}

		// Token: 0x060021F5 RID: 8693 RVA: 0x000151BD File Offset: 0x000141BD
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060021F6 RID: 8694 RVA: 0x000151CF File Offset: 0x000141CF
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000683 RID: 1667
		private _IExpression m_assCounterStart;

		// Token: 0x04000684 RID: 1668
		private _IExpression m_expUpper;

		// Token: 0x04000685 RID: 1669
		private _IStatement m_stateControlled;
	}
}
