using System;
using \u0001;
using \u0008;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001F
{
	// Token: 0x020003C0 RID: 960
	internal static class \u0015
	{
		// Token: 0x060036C8 RID: 14024 RVA: 0x000DE870 File Offset: 0x000DCA70
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			if (\u0004.MinimalSystem)
			{
				return;
			}
			global::\u0008.\u0012.\u0001(\u0013.QueryInterfacePointerMethodSignature.CreateCompiledSignature(null, \u0004.HasByteSupport()), \u0002, \u0003, \u0004, \u0005);
		}
	}
}
