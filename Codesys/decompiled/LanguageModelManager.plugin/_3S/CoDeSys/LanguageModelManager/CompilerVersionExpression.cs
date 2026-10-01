using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000047 RID: 71
	[TypeGuid("{1E3FEF00-B832-4dee-808D-06D2220A8354}")]
	[StorageVersion("3.3.0.0")]
	public class CompilerVersionExpression : PragmaExpression, _ICompilerVersionExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICompilerVersionExpression2, ICompilerVersionExpression, _IVersionComparisonSupportingExpression
	{
		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x060003D0 RID: 976 RVA: 0x0000C6CF File Offset: 0x0000B6CF
		// (set) Token: 0x060003D1 RID: 977 RVA: 0x0000C6DC File Offset: 0x0000B6DC
		[DefaultSerialization("Version")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string VersionToSave
		{
			get
			{
				return this.m_versionToTest.ToString();
			}
			set
			{
				this.m_versionToTest = new Version(value);
			}
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public CompilerVersionExpression()
		{
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public CompilerVersionExpression(IToken token) : base(token)
		{
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x0000C6FB File Offset: 0x0000B6FB
		public CompilerVersionExpression(Version versionToTest, Operator opComparison)
		{
			this.m_versionToTest = versionToTest;
			this.m_opComparison = opComparison;
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x0000C711 File Offset: 0x0000B711
		public CompilerVersionExpression(IToken token, Version versionToTest, Operator opComparison) : base(token)
		{
			this.m_versionToTest = versionToTest;
			this.m_opComparison = opComparison;
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060003D6 RID: 982 RVA: 0x0000C728 File Offset: 0x0000B728
		public Version VersionToTest
		{
			get
			{
				return this.m_versionToTest;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060003D7 RID: 983 RVA: 0x0000C730 File Offset: 0x0000B730
		public Operator OpComparison
		{
			get
			{
				return this.m_opComparison;
			}
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x0000C738 File Offset: 0x0000B738
		public void SetVersionToTest(Version version)
		{
			this.m_versionToTest = version;
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x0000C741 File Offset: 0x0000B741
		public void SetOpComparison(Operator op)
		{
			this.m_opComparison = op;
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0000C74A File Offset: 0x0000B74A
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060003DB RID: 987 RVA: 0x0000C753 File Offset: 0x0000B753
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060003DC RID: 988 RVA: 0x0000C75C File Offset: 0x0000B75C
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor4 exprVisitor = visitor as IExprVisitor4;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x060003DD RID: 989 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x060003DE RID: 990 RVA: 0x0000C770 File Offset: 0x0000B770
		public override _IExprement Duplicate()
		{
			CompilerVersionExpression compilerVersionExpression = new CompilerVersionExpression(this.m_versionToTest, this.m_opComparison);
			this.DuplicateCommon(compilerVersionExpression);
			return compilerVersionExpression;
		}

		// Token: 0x04000099 RID: 153
		private Version m_versionToTest;

		// Token: 0x0400009A RID: 154
		[DefaultSerialization("Operator")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Operator m_opComparison;
	}
}
