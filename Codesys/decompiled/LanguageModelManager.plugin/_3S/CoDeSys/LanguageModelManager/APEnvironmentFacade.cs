using System;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200001C RID: 28
	public static class APEnvironmentFacade
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000047 RID: 71 RVA: 0x000027FB File Offset: 0x000017FB
		// (set) Token: 0x06000048 RID: 72 RVA: 0x00002813 File Offset: 0x00001813
		public static IAPEnvironmentFacade Instance
		{
			get
			{
				if (APEnvironmentFacade.s_apEnvironmentFacade == null)
				{
					APEnvironmentFacade.s_apEnvironmentFacade = new APEnvironmentFacadeDesktop();
				}
				return APEnvironmentFacade.s_apEnvironmentFacade;
			}
			set
			{
				APEnvironmentFacade.s_apEnvironmentFacade = value;
			}
		}

		// Token: 0x0400000B RID: 11
		private static IAPEnvironmentFacade s_apEnvironmentFacade;
	}
}
