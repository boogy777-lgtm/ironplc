using System;
using \u0008;
using \u000F;
using \u0012;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0082;

namespace \u0014
{
	// Token: 0x020001DD RID: 477
	internal static class \u0008
	{
		// Token: 0x06002116 RID: 8470 RVA: 0x00070F6C File Offset: 0x0006F16C
		internal static void \u0001(LList<global::\u0008.\u0008> \u0002)
		{
			\u0002.Add(new \u0080.\u0010());
		}

		// Token: 0x06002117 RID: 8471 RVA: 0x00070F7C File Offset: 0x0006F17C
		internal static void \u0001(LList<global::\u0008.\u0008> \u0002, string \u0003, bool \u0004)
		{
			global::\u000F.\u0008 u = new global::\u000F.\u0008(\u0003, \u0004);
			\u0002.Add(u);
		}

		// Token: 0x06002118 RID: 8472 RVA: 0x00070F98 File Offset: 0x0006F198
		internal static void \u0001(LList<global::\u0008.\u0008> \u0002, string \u0003)
		{
			global::\u0012.\u000F u000F = new global::\u0012.\u000F(\u0003);
			\u0002.Add(u000F);
		}

		// Token: 0x06002119 RID: 8473 RVA: 0x00070FB4 File Offset: 0x0006F1B4
		internal static void \u0001(LList<global::\u0008.\u0008> \u0002, _IArrayType \u0003, int \u0004)
		{
			\u0082.\u0008 u = new \u0082.\u0008(\u0003, \u0004);
			\u0002.Add(u);
		}

		// Token: 0x0600211A RID: 8474 RVA: 0x00070FD0 File Offset: 0x0006F1D0
		internal static void \u0001(LList<global::\u0008.\u0008> \u0002, _IArrayType \u0003, LList<global::\u0008.\u0008> \u0004, int \u0005)
		{
			\u0082.\u000E u000E = new \u0082.\u000E(\u0003, \u0005, \u0004);
			\u0002.Add(u000E);
		}
	}
}
