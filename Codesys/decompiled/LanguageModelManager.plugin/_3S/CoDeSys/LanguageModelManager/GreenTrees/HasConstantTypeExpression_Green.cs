using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000227 RID: 551
	internal class HasConstantTypeExpression_Green : PragmaExpression_Green, _IHasConstantTypeExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasConstantTypeExpression
	{
		// Token: 0x06002434 RID: 9268 RVA: 0x0005C13E File Offset: 0x0005B13E
		public HasConstantTypeExpression_Green(_IExpression Constant, bool bConstantTypeReplaced)
		{
			this.m_Constant = Constant;
			this._ConstantTypeReplaced = bConstantTypeReplaced;
		}

		// Token: 0x17000A3B RID: 2619
		// (get) Token: 0x06002435 RID: 9269 RVA: 0x0005C154 File Offset: 0x0005B154
		public IExpression Constant
		{
			get
			{
				return this.m_Constant;
			}
		}

		// Token: 0x17000A3C RID: 2620
		// (get) Token: 0x06002436 RID: 9270 RVA: 0x0005C154 File Offset: 0x0005B154
		// (set) Token: 0x06002437 RID: 9271 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x17000A3D RID: 2621
		// (get) Token: 0x06002438 RID: 9272 RVA: 0x0005C15C File Offset: 0x0005B15C
		public bool ConstantTypeReplaced
		{
			get
			{
				return this._ConstantTypeReplaced;
			}
		}

		// Token: 0x17000A3E RID: 2622
		// (get) Token: 0x06002439 RID: 9273 RVA: 0x0005C164 File Offset: 0x0005B164
		// (set) Token: 0x0600243A RID: 9274 RVA: 0x0005C16C File Offset: 0x0005B16C
		public bool _ConstantTypeReplaced { get; set; }

		// Token: 0x0600243B RID: 9275 RVA: 0x0000EEA4 File Offset: 0x0000DEA4
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor351900 exprementVisitor = visitor as IExprementVisitor351900;
			if (exprementVisitor == null)
			{
				return;
			}
			exprementVisitor.visit(this);
		}

		// Token: 0x0600243C RID: 9276 RVA: 0x0005C178 File Offset: 0x0005B178
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor8 exprVisitor = visitor as IExprVisitor8;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x040006EC RID: 1772
		private readonly _IExpression m_Constant;
	}
}
