using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200008A RID: 138
	[TypeGuid("{d83e464d-5dd8-4bba-9349-a94f7efcfd06}")]
	[StorageVersion("3.3.0.0")]
	public class EmptyStatement : PositionStatement, _IEmptyStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IEmptyStatement
	{
		// Token: 0x060008A5 RID: 2213 RVA: 0x000149DB File Offset: 0x000139DB
		public EmptyStatement()
		{
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x000149E3 File Offset: 0x000139E3
		internal EmptyStatement(IToken token) : base(token)
		{
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x00014CBD File Offset: 0x00013CBD
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x00014CC6 File Offset: 0x00013CC6
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060008A9 RID: 2217 RVA: 0x00014CCF File Offset: 0x00013CCF
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008AA RID: 2218 RVA: 0x00014CD8 File Offset: 0x00013CD8
		public override _IExprement Duplicate()
		{
			EmptyStatement emptyStatement = new EmptyStatement();
			this.DuplicateCommon(emptyStatement);
			return emptyStatement;
		}

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x060008AB RID: 2219 RVA: 0x00005E58 File Offset: 0x00004E58
		public override bool KeepMessages
		{
			get
			{
				return true;
			}
		}
	}
}
