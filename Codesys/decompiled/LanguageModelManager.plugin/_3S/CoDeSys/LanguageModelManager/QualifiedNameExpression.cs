using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200006E RID: 110
	[TypeGuid("{14b16674-2feb-4df5-8503-ff7102c61004}")]
	[StorageVersion("3.3.0.0")]
	public class QualifiedNameExpression : PositionExpression, _IQualifiedNameExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IQualifiedNameExpression
	{
		// Token: 0x06000710 RID: 1808 RVA: 0x000125EF File Offset: 0x000115EF
		public QualifiedNameExpression()
		{
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x00012618 File Offset: 0x00011618
		public QualifiedNameExpression(string stName)
		{
			this.m_stName = stName;
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x00012648 File Offset: 0x00011648
		public QualifiedNameExpression(string stNamespace, string stName)
		{
			this.m_stNamespace = stNamespace;
			this.m_stName = stName;
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x0001267F File Offset: 0x0001167F
		internal QualifiedNameExpression(string stNamespace, string stName, IToken token) : base(token)
		{
			this.m_stNamespace = stNamespace;
			this.m_stName = stName;
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000715 RID: 1813 RVA: 0x000126B7 File Offset: 0x000116B7
		// (set) Token: 0x06000716 RID: 1814 RVA: 0x000126BF File Offset: 0x000116BF
		public string Name
		{
			get
			{
				return this.m_stName;
			}
			set
			{
				this.m_stName = value;
			}
		}

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x06000717 RID: 1815 RVA: 0x000126C8 File Offset: 0x000116C8
		// (set) Token: 0x06000718 RID: 1816 RVA: 0x000126D0 File Offset: 0x000116D0
		public string Namespace
		{
			get
			{
				return this.m_stNamespace;
			}
			set
			{
				this.m_stNamespace = value;
			}
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x000126D9 File Offset: 0x000116D9
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x000126E2 File Offset: 0x000116E2
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x0000E612 File Offset: 0x0000D612
		public override ISignature GetSignature(IScope scope)
		{
			return scope[this.SignatureId];
		}

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x0600071D RID: 1821 RVA: 0x000126EB File Offset: 0x000116EB
		// (set) Token: 0x0600071E RID: 1822 RVA: 0x000126F3 File Offset: 0x000116F3
		public override int SignatureId
		{
			get
			{
				return this.m_iSignatureId;
			}
			set
			{
				this.m_iSignatureId = value;
			}
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x000126FC File Offset: 0x000116FC
		public override _IExprement Duplicate()
		{
			QualifiedNameExpression qualifiedNameExpression = new QualifiedNameExpression(this.m_stNamespace, this.m_stName);
			qualifiedNameExpression.m_iSignatureId = this.m_iSignatureId;
			this.DuplicateCommon(qualifiedNameExpression);
			return qualifiedNameExpression;
		}

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000720 RID: 1824 RVA: 0x0001272F File Offset: 0x0001172F
		// (set) Token: 0x06000721 RID: 1825 RVA: 0x00012737 File Offset: 0x00011737
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x040000EF RID: 239
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		private ICompiledType m_ctype;

		// Token: 0x040000F0 RID: 240
		[DefaultSerialization("Namespace")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stNamespace = string.Empty;

		// Token: 0x040000F1 RID: 241
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stName = string.Empty;

		// Token: 0x040000F2 RID: 242
		[DefaultSerialization("SignatureId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iSignatureId = Common.InvalidID;
	}
}
