using System;
using \u0001;
using \u0008;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0014
{
	// Token: 0x020003BF RID: 959
	internal static class \u0015
	{
		// Token: 0x060036C7 RID: 14023 RVA: 0x000DE848 File Offset: 0x000DCA48
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			if (\u0004.MinimalSystem)
			{
				return;
			}
			global::\u0008.\u0012.\u0001(\u0013.QueryInterfaceMethodSignature.CreateCompiledSignature(null, \u0004.HasByteSupport()), \u0002, \u0003, \u0004, \u0005);
		}
	}
}
