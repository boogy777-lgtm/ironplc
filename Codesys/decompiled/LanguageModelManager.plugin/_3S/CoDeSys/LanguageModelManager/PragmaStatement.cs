using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200009E RID: 158
	[TypeGuid("{e8370761-0cbd-4aea-8cea-893206511371}")]
	[StorageVersion("3.3.0.0")]
	public class PragmaStatement : PositionStatement, _IPragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaStatement
	{
		// Token: 0x0600098E RID: 2446 RVA: 0x00016340 File Offset: 0x00015340
		public PragmaStatement()
		{
		}

		// Token: 0x0600098F RID: 2447 RVA: 0x00016353 File Offset: 0x00015353
		public PragmaStatement(IToken token) : base(token)
		{
		}

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06000990 RID: 2448 RVA: 0x00016367 File Offset: 0x00015367
		// (set) Token: 0x06000991 RID: 2449 RVA: 0x0001636F File Offset: 0x0001536F
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

		// Token: 0x06000992 RID: 2450 RVA: 0x00016378 File Offset: 0x00015378
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000993 RID: 2451 RVA: 0x00016381 File Offset: 0x00015381
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x0001638A File Offset: 0x0001538A
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x00016394 File Offset: 0x00015394
		public override _IExprement Duplicate()
		{
			PragmaStatement pragmaStatement = new PragmaStatement();
			this.DuplicateCommon(pragmaStatement);
			pragmaStatement.Text = this.Text;
			return pragmaStatement;
		}

		// Token: 0x04000155 RID: 341
		[DefaultSerialization("Text")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		protected string m_stText = string.Empty;
	}
}
