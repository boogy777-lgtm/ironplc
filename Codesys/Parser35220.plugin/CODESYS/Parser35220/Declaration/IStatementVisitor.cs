using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000053 RID: 83
	internal interface IStatementVisitor<out T>
	{
		// Token: 0x06000570 RID: 1392
		T visit(_ISequenceStatement statement);

		// Token: 0x06000571 RID: 1393
		T visit(_ICommentStatement statement);

		// Token: 0x06000572 RID: 1394
		T visit(_IPragmaStatement statement);

		// Token: 0x06000573 RID: 1395
		T visit(_IPragmaIfStatement statement);

		// Token: 0x06000574 RID: 1396
		T visit(_IVariableDeclarationStatement statement);

		// Token: 0x06000575 RID: 1397
		T visit(_IVariableDeclarationListStatement statement);
	}
}
