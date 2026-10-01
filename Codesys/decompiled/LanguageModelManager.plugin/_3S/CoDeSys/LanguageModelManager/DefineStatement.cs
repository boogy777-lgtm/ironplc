using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000087 RID: 135
	[TypeGuid("{f8e643c6-29ea-49ac-af49-6a64f7d10969}")]
	[StorageVersion("3.3.0.0")]
	public class DefineStatement : PositionStatement, _IDefineStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IDefineStatement
	{
		// Token: 0x0600087A RID: 2170 RVA: 0x000149DB File Offset: 0x000139DB
		public DefineStatement()
		{
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x00014A23 File Offset: 0x00013A23
		internal DefineStatement(IToken token, bool bDefine, string stIdent, string stValue) : base(token)
		{
			this.m_bDefine = bDefine;
			this.m_stIdent = stIdent;
			this.m_stValue = stValue;
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x00014A42 File Offset: 0x00013A42
		internal DefineStatement(IToken token, bool bDefine, string stIdent) : base(token)
		{
			this.m_bDefine = bDefine;
			this.m_stIdent = stIdent;
		}

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x0600087D RID: 2173 RVA: 0x00014A59 File Offset: 0x00013A59
		// (set) Token: 0x0600087E RID: 2174 RVA: 0x00014A61 File Offset: 0x00013A61
		public bool Define
		{
			get
			{
				return this.m_bDefine;
			}
			set
			{
				this.m_bDefine = value;
			}
		}

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x0600087F RID: 2175 RVA: 0x00014A6A File Offset: 0x00013A6A
		// (set) Token: 0x06000880 RID: 2176 RVA: 0x00014A72 File Offset: 0x00013A72
		public string Ident
		{
			get
			{
				return this.m_stIdent;
			}
			set
			{
				this.m_stIdent = value;
			}
		}

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x06000881 RID: 2177 RVA: 0x00014A7B File Offset: 0x00013A7B
		// (set) Token: 0x06000882 RID: 2178 RVA: 0x00014A83 File Offset: 0x00013A83
		public string Value
		{
			get
			{
				return this.m_stValue;
			}
			set
			{
				this.m_stValue = value;
			}
		}

		// Token: 0x06000883 RID: 2179 RVA: 0x00014A8C File Offset: 0x00013A8C
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x00014A95 File Offset: 0x00013A95
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x00014A9E File Offset: 0x00013A9E
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x00014AA8 File Offset: 0x00013AA8
		public override _IExprement Duplicate()
		{
			DefineStatement defineStatement = new DefineStatement();
			this.DuplicateCommon(defineStatement);
			defineStatement.m_bDefine = this.m_bDefine;
			defineStatement.m_stIdent = this.m_stIdent;
			defineStatement.m_stValue = this.m_stValue;
			return defineStatement;
		}

		// Token: 0x04000123 RID: 291
		[DefaultSerialization("Define")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bDefine;

		// Token: 0x04000124 RID: 292
		[DefaultSerialization("Ident")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stIdent;

		// Token: 0x04000125 RID: 293
		[DefaultSerialization("Attribute")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stValue;
	}
}
