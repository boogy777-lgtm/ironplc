using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000086 RID: 134
	[TypeGuid("{a7e5457c-055e-4707-9eb4-286c2df4e735}")]
	[StorageVersion("3.3.0.0")]
	public class ContinueStatement : PositionStatement, _IContinueStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IContinueStatement
	{
		// Token: 0x06000874 RID: 2164 RVA: 0x000149DB File Offset: 0x000139DB
		public ContinueStatement()
		{
		}

		// Token: 0x06000875 RID: 2165 RVA: 0x000149E3 File Offset: 0x000139E3
		public ContinueStatement(IToken token) : base(token)
		{
		}

		// Token: 0x06000876 RID: 2166 RVA: 0x000149EC File Offset: 0x000139EC
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x000149F5 File Offset: 0x000139F5
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000878 RID: 2168 RVA: 0x000149FE File Offset: 0x000139FE
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000879 RID: 2169 RVA: 0x00014A08 File Offset: 0x00013A08
		public override _IExprement Duplicate()
		{
			ContinueStatement continueStatement = new ContinueStatement();
			this.DuplicateCommon(continueStatement);
			return continueStatement;
		}
	}
}
