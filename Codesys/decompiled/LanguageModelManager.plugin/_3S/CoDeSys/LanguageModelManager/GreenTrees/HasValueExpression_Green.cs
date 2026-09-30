using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000225 RID: 549
	internal class HasValueExpression_Green : PragmaExpression_Green, _IHasValueExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IHasValueExpression
	{
		// Token: 0x06002420 RID: 9248 RVA: 0x0005C094 File Offset: 0x0005B094
		public HasValueExpression_Green(string stDefine, string stValue)
		{
			this.m_stDefine = stDefine;
			this.m_stValue = stValue;
		}

		// Token: 0x17000A32 RID: 2610
		// (get) Token: 0x06002421 RID: 9249 RVA: 0x0005C0AA File Offset: 0x0005B0AA
		// (set) Token: 0x06002422 RID: 9250 RVA: 0x0005A471 File Offset: 0x00059471
		public string Define
		{
			get
			{
				return this.m_stDefine;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A33 RID: 2611
		// (get) Token: 0x06002423 RID: 9251 RVA: 0x0005C0B2 File Offset: 0x0005B0B2
		// (set) Token: 0x06002424 RID: 9252 RVA: 0x0005A471 File Offset: 0x00059471
		public string DefineValue
		{
			get
			{
				return this.m_stValue;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002425 RID: 9253 RVA: 0x0000F14C File Offset: 0x0000E14C
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002426 RID: 9254 RVA: 0x0000F15E File Offset: 0x0000E15E
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x040006E7 RID: 1767
		private readonly string m_stDefine;

		// Token: 0x040006E8 RID: 1768
		private readonly string m_stValue;
	}
}
