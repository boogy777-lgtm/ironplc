using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000229 RID: 553
	internal class PragmaAssertion_Green : Statement_Green, _IPragmaAssertion, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaAssertion
	{
		// Token: 0x06002447 RID: 9287 RVA: 0x0005C20D File Offset: 0x0005B20D
		public PragmaAssertion_Green(_IExpression expCondition, string stErrorOutput)
		{
			this.m_expCondition = expCondition;
			this.m_stErrorOutput = stErrorOutput;
		}

		// Token: 0x17000A43 RID: 2627
		// (get) Token: 0x06002448 RID: 9288 RVA: 0x0005C223 File Offset: 0x0005B223
		// (set) Token: 0x06002449 RID: 9289 RVA: 0x0005A471 File Offset: 0x00059471
		public _IExpression Condition
		{
			get
			{
				if (this.m_expCondition == null)
				{
					return new NullExpression_Green();
				}
				return this.m_expCondition;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A44 RID: 2628
		// (get) Token: 0x0600244A RID: 9290 RVA: 0x0005C239 File Offset: 0x0005B239
		public IExpression ConditionExpression
		{
			get
			{
				return this.Condition;
			}
		}

		// Token: 0x17000A45 RID: 2629
		// (get) Token: 0x0600244B RID: 9291 RVA: 0x0005C241 File Offset: 0x0005B241
		// (set) Token: 0x0600244C RID: 9292 RVA: 0x0005A471 File Offset: 0x00059471
		public string ErrorOutput
		{
			get
			{
				return this.m_stErrorOutput;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0600244D RID: 9293 RVA: 0x00016008 File Offset: 0x00015008
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600244E RID: 9294 RVA: 0x0001601A File Offset: 0x0001501A
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006F0 RID: 1776
		private readonly string m_stErrorOutput;

		// Token: 0x040006F1 RID: 1777
		private readonly _IExpression m_expCondition;
	}
}
