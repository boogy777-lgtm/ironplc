using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200021A RID: 538
	internal class TaskReference_Green : ItemReference_Green, _ITaskReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060023D0 RID: 9168 RVA: 0x0005BE58 File Offset: 0x0005AE58
		public TaskReference_Green(string stTaskName)
		{
			this.m_stTaskName = stTaskName;
		}

		// Token: 0x17000A18 RID: 2584
		// (get) Token: 0x060023D1 RID: 9169 RVA: 0x0005BE67 File Offset: 0x0005AE67
		// (set) Token: 0x060023D2 RID: 9170 RVA: 0x0005A471 File Offset: 0x00059471
		public string TaskName
		{
			get
			{
				return this.m_stTaskName;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060023D3 RID: 9171 RVA: 0x00012EAA File Offset: 0x00011EAA
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023D4 RID: 9172 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x040006D7 RID: 1751
		private readonly string m_stTaskName;
	}
}
