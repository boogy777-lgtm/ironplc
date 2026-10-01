using System;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0081
{
	// Token: 0x02000160 RID: 352
	internal sealed class \u000E : _ICompilerVersionSettings
	{
		// Token: 0x0600183B RID: 6203 RVA: 0x0004BC34 File Offset: 0x00049E34
		internal \u000E(Version \u0089\u0008)
		{
			this.\u0001 = \u0089\u0008;
		}

		// Token: 0x0600183C RID: 6204 RVA: 0x0004BC44 File Offset: 0x00049E44
		public Version \u0001()
		{
			return this.\u0001;
		}

		// Token: 0x0600183D RID: 6205 RVA: 0x0004BC4C File Offset: 0x00049E4C
		public string \u0001(Version \u0002)
		{
			return APEnvironmentFacade.Instance.CompilerVersionSettings.MapFromInternalToOEMText(\u0002);
		}

		// Token: 0x0600183E RID: 6206 RVA: 0x0004BC60 File Offset: 0x00049E60
		public string \u0002(Version \u0002)
		{
			return APEnvironmentFacade.Instance.CompilerVersionSettings.MapFromInternalToOEMTextSave(\u0002);
		}

		// Token: 0x04000449 RID: 1097
		private readonly Version \u0001;
	}
}
