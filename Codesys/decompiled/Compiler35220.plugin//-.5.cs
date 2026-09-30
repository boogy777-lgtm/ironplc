using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x0200010F RID: 271
	internal sealed class \u0005 : IComparer<ICompiledPOU4>
	{
		// Token: 0x06001411 RID: 5137 RVA: 0x0003AFDC File Offset: 0x000391DC
		public int \u0001(ICompiledPOU4 \u0002, ICompiledPOU4 \u0003)
		{
			int numberOfStatements = ((_ICompiledPOU)\u0002).NumberOfStatements;
			int numberOfStatements2 = ((_ICompiledPOU)\u0003).NumberOfStatements;
			if (numberOfStatements < numberOfStatements2)
			{
				return 1;
			}
			if (numberOfStatements > numberOfStatements2)
			{
				return -1;
			}
			return 0;
		}
	}
}
