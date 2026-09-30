using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200021B RID: 539
	internal class ResourceReference_Green : ItemReference_Green, _IResourceReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060023D5 RID: 9173 RVA: 0x0005BE6F File Offset: 0x0005AE6F
		public ResourceReference_Green(string stResourceName)
		{
			this.m_stResourceName = stResourceName;
		}

		// Token: 0x17000A19 RID: 2585
		// (get) Token: 0x060023D6 RID: 9174 RVA: 0x0005BE7E File Offset: 0x0005AE7E
		// (set) Token: 0x060023D7 RID: 9175 RVA: 0x0005A471 File Offset: 0x00059471
		public string ResourceName
		{
			get
			{
				return this.m_stResourceName;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060023D8 RID: 9176 RVA: 0x00012761 File Offset: 0x00011761
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023D9 RID: 9177 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x040006D8 RID: 1752
		private readonly string m_stResourceName;
	}
}
