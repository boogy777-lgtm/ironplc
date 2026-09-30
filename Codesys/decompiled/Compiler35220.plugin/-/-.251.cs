using System;
using \u0006;
using \u000E;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0010
{
	// Token: 0x020002B3 RID: 691
	internal static class \u0006
	{
		// Token: 0x06002AB9 RID: 10937 RVA: 0x00096244 File Offset: 0x00094444
		public static _IStatement \u0001(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_IStatement st = new global::\u0006.\u000E(\u0003._Scope, \u0003.Comcon).\u0001(\u0002, \u0003);
			return \u0003.Generator.DisableFlowBPForallExceptFirst(st, \u0002._Position);
		}
	}
}
