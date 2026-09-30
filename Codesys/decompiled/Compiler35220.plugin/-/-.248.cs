using System;
using \u000E;
using \u0016;
using \u0019;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0011
{
	// Token: 0x020002AD RID: 685
	internal static class \u0011
	{
		// Token: 0x06002A9B RID: 10907 RVA: 0x000953A0 File Offset: 0x000935A0
		public static _IStatement \u0001(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			if (!global::\u0016.\u0004.MemoryBarrier.GetBoolValue(\u0003.Comcon.GetTargetSettings()))
			{
				return \u0019.\u0003.Builder.CreateEmptyStatement();
			}
			return null;
		}
	}
}
