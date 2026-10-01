using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200021E RID: 542
	internal class CompilerVersionExpression_Green : Expression_Green, _ICompilerVersionExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICompilerVersionExpression2, ICompilerVersionExpression, _IVersionComparisonSupportingExpression
	{
		// Token: 0x060023E8 RID: 9192 RVA: 0x0005BF03 File Offset: 0x0005AF03
		public CompilerVersionExpression_Green(Version versionToTest, Operator opComparison)
		{
			this.m_versionToTest = versionToTest;
			this.m_opComparison = opComparison;
		}

		// Token: 0x17000A1E RID: 2590
		// (get) Token: 0x060023E9 RID: 9193 RVA: 0x0005BF19 File Offset: 0x0005AF19
		public Version VersionToTest
		{
			get
			{
				return this.m_versionToTest;
			}
		}

		// Token: 0x17000A1F RID: 2591
		// (get) Token: 0x060023EA RID: 9194 RVA: 0x0005BF21 File Offset: 0x0005AF21
		public Operator OpComparison
		{
			get
			{
				return this.m_opComparison;
			}
		}

		// Token: 0x17000A20 RID: 2592
		// (get) Token: 0x060023EB RID: 9195 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x060023EC RID: 9196 RVA: 0x0005A471 File Offset: 0x00059471
		public bool Value
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x17000A21 RID: 2593
		// (get) Token: 0x060023ED RID: 9197 RVA: 0x0005A471 File Offset: 0x00059471
		// (set) Token: 0x060023EE RID: 9198 RVA: 0x0005A471 File Offset: 0x00059471
		public bool ValueStillUndecided
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x060023EF RID: 9199 RVA: 0x0005BF29 File Offset: 0x0005AF29
		public void SetVersionToTest(Version version)
		{
			this.m_versionToTest = version;
		}

		// Token: 0x060023F0 RID: 9200 RVA: 0x0005BF32 File Offset: 0x0005AF32
		public void SetOpComparison(Operator op)
		{
			this.m_opComparison = op;
		}

		// Token: 0x060023F1 RID: 9201 RVA: 0x0000C74A File Offset: 0x0000B74A
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060023F2 RID: 9202 RVA: 0x0000C75C File Offset: 0x0000B75C
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor4 exprVisitor = visitor as IExprVisitor4;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x040006DC RID: 1756
		private Version m_versionToTest;

		// Token: 0x040006DD RID: 1757
		private Operator m_opComparison;
	}
}
