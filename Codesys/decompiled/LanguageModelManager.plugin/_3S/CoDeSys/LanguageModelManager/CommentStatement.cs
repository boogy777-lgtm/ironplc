using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000085 RID: 133
	[TypeGuid("{b7de5660-14db-4c90-8e14-ce80cd92ce3b}")]
	[StorageVersion("3.3.0.0")]
	public class CommentStatement : PositionStatement, _ICommentStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICommentStatement
	{
		// Token: 0x06000869 RID: 2153 RVA: 0x00014920 File Offset: 0x00013920
		public CommentStatement()
		{
		}

		// Token: 0x0600086A RID: 2154 RVA: 0x00014933 File Offset: 0x00013933
		internal CommentStatement(string stText)
		{
			this.m_stText = stText;
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x0001494D File Offset: 0x0001394D
		internal CommentStatement(string stText, IToken token) : base(token)
		{
			this.m_stText = stText;
		}

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x0600086C RID: 2156 RVA: 0x00014968 File Offset: 0x00013968
		// (set) Token: 0x0600086D RID: 2157 RVA: 0x00014970 File Offset: 0x00013970
		public string Text
		{
			get
			{
				return this.m_stText;
			}
			set
			{
				this.m_stText = value;
			}
		}

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x0600086E RID: 2158 RVA: 0x00014979 File Offset: 0x00013979
		// (set) Token: 0x0600086F RID: 2159 RVA: 0x00014981 File Offset: 0x00013981
		public bool DocComment
		{
			get
			{
				return this.m_bDocComment;
			}
			set
			{
				this.m_bDocComment = value;
			}
		}

		// Token: 0x06000870 RID: 2160 RVA: 0x0001498A File Offset: 0x0001398A
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000871 RID: 2161 RVA: 0x00014993 File Offset: 0x00013993
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000872 RID: 2162 RVA: 0x0001499C File Offset: 0x0001399C
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000873 RID: 2163 RVA: 0x000149A8 File Offset: 0x000139A8
		public override _IExprement Duplicate()
		{
			CommentStatement commentStatement = new CommentStatement();
			this.DuplicateCommon(commentStatement);
			commentStatement.m_stText = this.m_stText;
			commentStatement.m_bDocComment = this.m_bDocComment;
			return commentStatement;
		}

		// Token: 0x04000121 RID: 289
		[DefaultSerialization("Text")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stText = string.Empty;

		// Token: 0x04000122 RID: 290
		[DefaultSerialization("DocComment")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bDocComment;
	}
}
