using System;
using \u0008;
using \u000F;
using \u0012;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0083;

namespace \u0084
{
	// Token: 0x020001DF RID: 479
	internal static class \u0010
	{
		// Token: 0x06002125 RID: 8485 RVA: 0x00071154 File Offset: 0x0006F354
		public static void \u0001(LList<global::\u0008.\u0008> \u0002, LList<global::\u0008.\u0008> \u0003)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				global::\u0008.\u0008 u = \u0002[i];
				global::\u0008.\u0008 u2 = (i < \u0002.Count - 1) ? \u0002[i + 1] : null;
				global::\u0008.\u0008 u3 = (i < \u0002.Count - 2) ? \u0002[i + 2] : null;
				if (u is \u0080.\u0010 && u2 is global::\u0012.\u000F && u3 is global::\u000F.\u0008)
				{
					\u0083.\u0004 u4 = new \u0083.\u0004((u3 as global::\u000F.\u0008).LValue, (u2 as global::\u0012.\u000F).Literal, (u3 as global::\u000F.\u0008).AddedViaOnlineChange);
					\u0003.Add(u4);
					i += 2;
				}
				else
				{
					\u0003.Add(u);
				}
			}
		}
	}
}
