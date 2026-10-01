using System;
using \u000F;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0084
{
	// Token: 0x0200020B RID: 523
	internal sealed class \u0012 : AbstractToVisitchecker
	{
		// Token: 0x06002250 RID: 8784 RVA: 0x000777F0 File Offset: 0x000759F0
		public override bool ToVisit(_IOperatorExpression op)
		{
			return \u0010.\u0001(op.Code);
		}
	}
}
