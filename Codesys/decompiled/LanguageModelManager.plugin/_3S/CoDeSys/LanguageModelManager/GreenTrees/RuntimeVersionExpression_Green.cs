using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200021F RID: 543
	internal class RuntimeVersionExpression_Green : PragmaExpression_Green, _IRuntimeVersionExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IRuntimeVersionExpression, _IVersionComparisonSupportingExpression
	{
		// Token: 0x060023F3 RID: 9203 RVA: 0x0005BF3B File Offset: 0x0005AF3B
		public RuntimeVersionExpression_Green(Version versionToTest, Operator opComparison)
		{
			this.m_versionToTest = versionToTest;
			this.m_opComparison = opComparison;
		}

		// Token: 0x17000A22 RID: 2594
		// (get) Token: 0x060023F4 RID: 9204 RVA: 0x0005BF51 File Offset: 0x0005AF51
		public Version VersionToTest
		{
			get
			{
				return this.m_versionToTest;
			}
		}

		// Token: 0x17000A23 RID: 2595
		// (get) Token: 0x060023F5 RID: 9205 RVA: 0x0005BF59 File Offset: 0x0005AF59
		public Operator OpComparison
		{
			get
			{
				return this.m_opComparison;
			}
		}

		// Token: 0x060023F6 RID: 9206 RVA: 0x0005BF61 File Offset: 0x0005AF61
		public void SetVersionToTest(Version version)
		{
			this.m_versionToTest = version;
		}

		// Token: 0x060023F7 RID: 9207 RVA: 0x0005BF6A File Offset: 0x0005AF6A
		public void SetOpComparison(Operator op)
		{
			this.m_opComparison = op;
		}

		// Token: 0x060023F8 RID: 9208 RVA: 0x00012805 File Offset: 0x00011805
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor351400 exprementVisitor = visitor as IExprementVisitor351400;
			if (exprementVisitor == null)
			{
				return;
			}
			exprementVisitor.visit(this);
		}

		// Token: 0x060023F9 RID: 9209 RVA: 0x00012821 File Offset: 0x00011821
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor4 exprVisitor = visitor as IExprVisitor4;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x040006DE RID: 1758
		private Version m_versionToTest;

		// Token: 0x040006DF RID: 1759
		private Operator m_opComparison;
	}
}
