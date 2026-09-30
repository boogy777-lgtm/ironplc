using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200027A RID: 634
	public interface IStatementVisitor<out T>
	{
		// Token: 0x0600281B RID: 10267
		T visit(_IWhileStatement whilst);

		// Token: 0x0600281C RID: 10268
		T visit(_IRepeatStatement repeat);

		// Token: 0x0600281D RID: 10269
		T visit(_IForStatement forloop);

		// Token: 0x0600281E RID: 10270
		T visit(_IIfStatement ifst);

		// Token: 0x0600281F RID: 10271
		T visit(_IExpressionStatement expstat);

		// Token: 0x06002820 RID: 10272
		T visit(_ICaseStatement caseStatement);

		// Token: 0x06002821 RID: 10273
		T visit(_ISequenceStatement sequenceStatement);

		// Token: 0x06002822 RID: 10274
		T visit(_IPragmaStatement pragmaStatement);

		// Token: 0x06002823 RID: 10275
		T visit(_IReturnStatement returnStatement);

		// Token: 0x06002824 RID: 10276
		T visit(_ITryCatchStatement tryCatchStatement);

		// Token: 0x06002825 RID: 10277
		T visitGeneric(_IStatement statement);
	}
}
