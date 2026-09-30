using System;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0013
{
	// Token: 0x020002C8 RID: 712
	internal sealed class \u0008 : AbstractToVisitchecker
	{
		// Token: 0x06002B2A RID: 11050 RVA: 0x000981D8 File Offset: 0x000963D8
		public override bool ToVisit(_IOperatorExpression op)
		{
			return SpecialOperationsReplacer.\u0001(op.Code);
		}
	}
}
