using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200008D RID: 141
	[TypeGuid("{a7bed6b0-097f-4871-bcdd-7298a0c44e7f}")]
	[StorageVersion("3.3.0.0")]
	public class ErrorStatement : PositionStatement, IErrorStatement, IStatement, IExprement, _IErrorStatement, _IStatement, _IExprement, IExprement3, IExprement2
	{
		// Token: 0x060008CA RID: 2250 RVA: 0x000149DB File Offset: 0x000139DB
		public ErrorStatement()
		{
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x000149E3 File Offset: 0x000139E3
		internal ErrorStatement(IToken token) : base(token)
		{
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x00014FF2 File Offset: 0x00013FF2
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x00014FFB File Offset: 0x00013FFB
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x00015004 File Offset: 0x00014004
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor6 exprVisitor = visitor as IExprVisitor6;
			if (exprVisitor == null)
			{
				return;
			}
			exprVisitor.visit(this);
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x00015018 File Offset: 0x00014018
		public override _IExprement Duplicate()
		{
			ErrorStatement errorStatement = new ErrorStatement();
			this.DuplicateCommon(errorStatement);
			return errorStatement;
		}
	}
}
