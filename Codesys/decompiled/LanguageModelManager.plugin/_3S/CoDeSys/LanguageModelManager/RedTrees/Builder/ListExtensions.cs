using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder
{
	// Token: 0x02000263 RID: 611
	public static class ListExtensions
	{
		// Token: 0x060029C8 RID: 10696 RVA: 0x0006AA9C File Offset: 0x00069A9C
		public static ISequenceStatement2 IntoSequence(this IEnumerable<IStatement> statements)
		{
			return LanguageModelBuilder.Singleton.CreateSequenceStatementEx(null, statements);
		}

		// Token: 0x060029C9 RID: 10697 RVA: 0x0006AAAA File Offset: 0x00069AAA
		public static ICaseLabelStatement IntoCaseLabel(this IEnumerable<IExpression> cases)
		{
			return LanguageModelBuilder.Singleton.CreateCaseLabelStatement(null, cases.ToList<IExpression>());
		}
	}
}
