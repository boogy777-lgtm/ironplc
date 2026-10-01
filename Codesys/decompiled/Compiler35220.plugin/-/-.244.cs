using System;
using \u000E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0008
{
	// Token: 0x020002A6 RID: 678
	internal static class \u0011
	{
		// Token: 0x06002A8B RID: 10891 RVA: 0x00094D7C File Offset: 0x00092F7C
		public static _IExpression \u0001(_IOperatorExpression \u0002, \u0011 \u0003)
		{
			if (!(\u0002[0].GetVariable(\u0003._Scope) is _IVariable))
			{
				throw new LateCompileErrorException();
			}
			string stInput = \u0002[0].ToString() + ".FB_INIT(" + \u0002[1].ToString() + ", FALSE)";
			return \u0003.Generator.GenerateExpression(stInput, \u0003._Scope, \u0003.CompiledPOU);
		}
	}
}
