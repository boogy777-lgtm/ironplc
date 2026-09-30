using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001EF RID: 495
	internal class PragmaStatement_Green : Statement_Green, _IPragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaStatement
	{
		// Token: 0x06002254 RID: 8788 RVA: 0x0005ABF8 File Offset: 0x00059BF8
		public PragmaStatement_Green(string stText)
		{
			this.m_stText = stText;
		}

		// Token: 0x17000989 RID: 2441
		// (get) Token: 0x06002255 RID: 8789 RVA: 0x0005AC07 File Offset: 0x00059C07
		// (set) Token: 0x06002256 RID: 8790 RVA: 0x0005A471 File Offset: 0x00059471
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

		// Token: 0x06002257 RID: 8791 RVA: 0x00016378 File Offset: 0x00015378
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002258 RID: 8792 RVA: 0x0001638A File Offset: 0x0001538A
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000697 RID: 1687
		protected string m_stText;
	}
}
