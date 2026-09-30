using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001ED RID: 493
	internal class LabelStatement_Green : Statement_Green, _ILabelStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ILabelStatement2, ILabelStatement
	{
		// Token: 0x06002247 RID: 8775 RVA: 0x0005AB96 File Offset: 0x00059B96
		public LabelStatement_Green(string stLabel)
		{
			this.m_stLabel = stLabel;
		}

		// Token: 0x17000985 RID: 2437
		// (get) Token: 0x06002248 RID: 8776 RVA: 0x0005ABA5 File Offset: 0x00059BA5
		// (set) Token: 0x06002249 RID: 8777 RVA: 0x0005A471 File Offset: 0x00059471
		public string Text
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3211)
				{
					return this.m_stLabel.ToUpperInvariant();
				}
				return this.m_stLabel;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000986 RID: 2438
		// (get) Token: 0x0600224A RID: 8778 RVA: 0x0005ABCA File Offset: 0x00059BCA
		public string OrgText
		{
			get
			{
				return this.m_stLabel;
			}
		}

		// Token: 0x0600224B RID: 8779 RVA: 0x000156BB File Offset: 0x000146BB
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600224C RID: 8780 RVA: 0x000156CD File Offset: 0x000146CD
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x04000694 RID: 1684
		private readonly string m_stLabel;
	}
}
