using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001EE RID: 494
	internal class CommentStatement_Green : Statement_Green, _ICommentStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICommentStatement
	{
		// Token: 0x0600224D RID: 8781 RVA: 0x0005ABD2 File Offset: 0x00059BD2
		internal CommentStatement_Green(string stText, bool bDocComment)
		{
			this.m_stText = stText;
			this.m_bDocComment = bDocComment;
		}

		// Token: 0x17000987 RID: 2439
		// (get) Token: 0x0600224E RID: 8782 RVA: 0x0005ABE8 File Offset: 0x00059BE8
		// (set) Token: 0x0600224F RID: 8783 RVA: 0x0005A471 File Offset: 0x00059471
		public string Text
		{
			get
			{
				return this.m_stText;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000988 RID: 2440
		// (get) Token: 0x06002250 RID: 8784 RVA: 0x0005ABF0 File Offset: 0x00059BF0
		// (set) Token: 0x06002251 RID: 8785 RVA: 0x0005A471 File Offset: 0x00059471
		public bool DocComment
		{
			get
			{
				return this.m_bDocComment;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x06002252 RID: 8786 RVA: 0x0001498A File Offset: 0x0001398A
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002253 RID: 8787 RVA: 0x0001499C File Offset: 0x0001399C
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000695 RID: 1685
		private readonly string m_stText;

		// Token: 0x04000696 RID: 1686
		private readonly bool m_bDocComment;
	}
}
