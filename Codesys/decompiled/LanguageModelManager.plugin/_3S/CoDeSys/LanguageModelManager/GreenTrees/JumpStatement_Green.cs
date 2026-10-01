using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001EC RID: 492
	internal class JumpStatement_Green : Statement_Green, _IJumpStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IJumpStatement
	{
		// Token: 0x0600223F RID: 8767 RVA: 0x0005AB4B File Offset: 0x00059B4B
		public JumpStatement_Green(_IExpression expCondition, string stLabel)
		{
			this.m_expCondition = expCondition;
			this.m_stLabel = stLabel;
		}

		// Token: 0x17000982 RID: 2434
		// (get) Token: 0x06002240 RID: 8768 RVA: 0x0005AB61 File Offset: 0x00059B61
		// (set) Token: 0x06002241 RID: 8769 RVA: 0x0005A471 File Offset: 0x00059471
		public string Label
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

		// Token: 0x06002242 RID: 8770 RVA: 0x000155C1 File Offset: 0x000145C1
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06002243 RID: 8771 RVA: 0x000155D3 File Offset: 0x000145D3
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x17000983 RID: 2435
		// (get) Token: 0x06002244 RID: 8772 RVA: 0x0005AB86 File Offset: 0x00059B86
		public IExpression Condition
		{
			get
			{
				return this._Condition;
			}
		}

		// Token: 0x17000984 RID: 2436
		// (get) Token: 0x06002245 RID: 8773 RVA: 0x0005AB8E File Offset: 0x00059B8E
		// (set) Token: 0x06002246 RID: 8774 RVA: 0x0000677E File Offset: 0x0000577E
		public _IExpression _Condition
		{
			get
			{
				return this.m_expCondition;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		// Token: 0x04000692 RID: 1682
		private readonly string m_stLabel;

		// Token: 0x04000693 RID: 1683
		private readonly _IExpression m_expCondition;
	}
}
