using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000226 RID: 550
	internal class HasConstantValueExpression_Green : PragmaExpression_Green, _IHasConstantValueExpression2, _IHasConstantValueExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasConstantValueExpression2, IHasConstantValueExpression, IHasConstantValueExpression3
	{
		// Token: 0x06002427 RID: 9255 RVA: 0x0005C0BA File Offset: 0x0005B0BA
		public HasConstantValueExpression_Green(_IExpression Constant, _IExpression Value, Operator opComparison)
		{
			this.m_Constant = Constant;
			this._ConstantValue = Value;
			this._OpComparison = opComparison;
		}

		// Token: 0x17000A34 RID: 2612
		// (get) Token: 0x06002428 RID: 9256 RVA: 0x0005C0D7 File Offset: 0x0005B0D7
		public IExpression Constant
		{
			get
			{
				return this.m_Constant;
			}
		}

		// Token: 0x17000A35 RID: 2613
		// (get) Token: 0x06002429 RID: 9257 RVA: 0x0005C0D7 File Offset: 0x0005B0D7
		// (set) Token: 0x0600242A RID: 9258 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression _Constant
		{
			get
			{
				return this.m_Constant;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A36 RID: 2614
		// (get) Token: 0x0600242B RID: 9259 RVA: 0x0005C0DF File Offset: 0x0005B0DF
		public ILiteralExpression ConstantValue
		{
			get
			{
				return this._ConstantValue as ILiteralExpression;
			}
		}

		// Token: 0x17000A37 RID: 2615
		// (get) Token: 0x0600242C RID: 9260 RVA: 0x0005C0EC File Offset: 0x0005B0EC
		public IExpression ConstantValueExpression
		{
			get
			{
				return this._ConstantValue;
			}
		}

		// Token: 0x17000A38 RID: 2616
		// (get) Token: 0x0600242D RID: 9261 RVA: 0x0005C0F4 File Offset: 0x0005B0F4
		// (set) Token: 0x0600242E RID: 9262 RVA: 0x0005C0FC File Offset: 0x0005B0FC
		public _IExpression _ConstantValue { get; set; }

		// Token: 0x17000A39 RID: 2617
		// (get) Token: 0x0600242F RID: 9263 RVA: 0x0005C105 File Offset: 0x0005B105
		public Operator OpComparison
		{
			get
			{
				return this._OpComparison;
			}
		}

		// Token: 0x17000A3A RID: 2618
		// (get) Token: 0x06002430 RID: 9264 RVA: 0x0005C10D File Offset: 0x0005B10D
		// (set) Token: 0x06002431 RID: 9265 RVA: 0x0005C115 File Offset: 0x0005B115
		public Operator _OpComparison { get; set; }

		// Token: 0x06002432 RID: 9266 RVA: 0x0000EFAE File Offset: 0x0000DFAE
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002433 RID: 9267 RVA: 0x0005C120 File Offset: 0x0005B120
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor2 exprVisitor = visitor as IExprVisitor2;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x040006E9 RID: 1769
		private readonly _IExpression m_Constant;
	}
}
