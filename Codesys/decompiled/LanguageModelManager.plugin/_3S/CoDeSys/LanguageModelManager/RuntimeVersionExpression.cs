using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000070 RID: 112
	[TypeGuid("{E4B16445-583D-4075-B2B0-B4CD2B63EF36}")]
	[StorageVersion("3.5.14.0")]
	public class RuntimeVersionExpression : PragmaExpression, _IRuntimeVersionExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IRuntimeVersionExpression, _IVersionComparisonSupportingExpression
	{
		// Token: 0x17000193 RID: 403
		// (get) Token: 0x0600072C RID: 1836 RVA: 0x0001279B File Offset: 0x0001179B
		// (set) Token: 0x0600072D RID: 1837 RVA: 0x000127A8 File Offset: 0x000117A8
		[DefaultSerialization("Version")]
		[StorageVersion("3.5.14.0")]
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

		// Token: 0x0600072E RID: 1838 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public RuntimeVersionExpression()
		{
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public RuntimeVersionExpression(IToken token) : base(token)
		{
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x000127B6 File Offset: 0x000117B6
		public RuntimeVersionExpression(Version versionToTest, Operator opComparison)
		{
			this.m_versionToTest = versionToTest;
			this.m_opComparison = opComparison;
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x000127CC File Offset: 0x000117CC
		public RuntimeVersionExpression(IToken token, Version versionToTest, Operator opComparison) : base(token)
		{
			this.m_versionToTest = versionToTest;
			this.m_opComparison = opComparison;
		}

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000732 RID: 1842 RVA: 0x000127E3 File Offset: 0x000117E3
		public Version VersionToTest
		{
			get
			{
				return this.m_versionToTest;
			}
		}

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000733 RID: 1843 RVA: 0x000127EB File Offset: 0x000117EB
		public Operator OpComparison
		{
			get
			{
				return this.m_opComparison;
			}
		}

		// Token: 0x06000734 RID: 1844 RVA: 0x000127F3 File Offset: 0x000117F3
		public void SetVersionToTest(Version version)
		{
			this.m_versionToTest = version;
		}

		// Token: 0x06000735 RID: 1845 RVA: 0x000127FC File Offset: 0x000117FC
		public void SetOpComparison(Operator op)
		{
			this.m_opComparison = op;
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x00012805 File Offset: 0x00011805
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor351400 exprementVisitor = visitor as IExprementVisitor351400;
			if (exprementVisitor == null)
			{
				return;
			}
			exprementVisitor.visit(this);
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x00012818 File Offset: 0x00011818
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x00012821 File Offset: 0x00011821
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor4 exprVisitor = visitor as IExprVisitor4;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x00012834 File Offset: 0x00011834
		public override _IExprement Duplicate()
		{
			RuntimeVersionExpression runtimeVersionExpression = new RuntimeVersionExpression(this.m_versionToTest, this.m_opComparison);
			this.DuplicateCommon(runtimeVersionExpression);
			return runtimeVersionExpression;
		}

		// Token: 0x040000F4 RID: 244
		private Version m_versionToTest;

		// Token: 0x040000F5 RID: 245
		[DefaultSerialization("Operator")]
		[StorageVersion("3.5.14.0")]
		[Obfuscation(Feature = "rename")]
		private Operator m_opComparison;
	}
}
