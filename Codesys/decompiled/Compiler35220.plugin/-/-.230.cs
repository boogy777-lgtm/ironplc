using System;
using \u0008;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0003
{
	// Token: 0x02000289 RID: 649
	internal class \u0011 : global::\u0008.\u0010
	{
		// Token: 0x060028D6 RID: 10454 RVA: 0x0008EE90 File Offset: 0x0008D090
		protected \u0011(\u0081.\u0010 \u0096\u0007, ISpecificExpressionReplacer \u0018\u0006, global::\u000E.\u0011 \u0083\u0005) : base(\u0096\u0007, \u0018\u0006, \u0083\u0005)
		{
		}

		// Token: 0x060028D7 RID: 10455 RVA: 0x0008EE9C File Offset: 0x0008D09C
		protected override _IExpression \u0001(_IExpression \u0002, bool \u0003 = true)
		{
			\u0002.Accept(this);
			return this.\u0001.ReplaceExpression(\u0002, \u0003);
		}
	}
}
