using System;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x0200027D RID: 637
	internal sealed class \u000F : AbstractToVisitchecker
	{
		// Token: 0x06002835 RID: 10293 RVA: 0x0008BB98 File Offset: 0x00089D98
		public override bool ToVisit(_IIfStatement ifst)
		{
			return ifst._ElseIf != null && ifst._ElseIf.Count > 0;
		}
	}
}
