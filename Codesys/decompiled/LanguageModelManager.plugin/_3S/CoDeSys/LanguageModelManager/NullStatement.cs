using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000098 RID: 152
	[TypeGuid("{75679ba4-8963-4e5f-afdd-74ab9cbcdd98}")]
	[StorageVersion("3.3.0.0")]
	public class NullStatement : PositionStatement, _INullStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement
	{
		// Token: 0x06000939 RID: 2361 RVA: 0x000149DB File Offset: 0x000139DB
		public NullStatement()
		{
		}

		// Token: 0x0600093A RID: 2362 RVA: 0x000149E3 File Offset: 0x000139E3
		internal NullStatement(IToken token) : base(token)
		{
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x00015940 File Offset: 0x00014940
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x00015949 File Offset: 0x00014949
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x0600093E RID: 2366 RVA: 0x00015954 File Offset: 0x00014954
		public override _IExprement Duplicate()
		{
			NullStatement nullStatement = new NullStatement();
			this.DuplicateCommon(nullStatement);
			return nullStatement;
		}
	}
}
