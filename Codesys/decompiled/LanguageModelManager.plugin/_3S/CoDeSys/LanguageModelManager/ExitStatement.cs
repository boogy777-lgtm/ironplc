using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200008E RID: 142
	[TypeGuid("{7b7cf220-a3ca-4599-bf62-49577e290eff}")]
	[StorageVersion("3.3.0.0")]
	public class ExitStatement : PositionStatement, _IExitStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IExitStatement
	{
		// Token: 0x060008D0 RID: 2256 RVA: 0x000149DB File Offset: 0x000139DB
		public ExitStatement()
		{
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x000149E3 File Offset: 0x000139E3
		public ExitStatement(IToken token) : base(token)
		{
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x00015033 File Offset: 0x00014033
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x0001503C File Offset: 0x0001403C
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x00015045 File Offset: 0x00014045
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x00015050 File Offset: 0x00014050
		public override _IExprement Duplicate()
		{
			ExitStatement exitStatement = new ExitStatement();
			this.DuplicateCommon(exitStatement);
			return exitStatement;
		}
	}
}
