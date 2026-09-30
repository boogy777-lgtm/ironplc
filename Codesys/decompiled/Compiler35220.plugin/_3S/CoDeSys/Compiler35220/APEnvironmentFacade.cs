using System;

namespace _3S.CoDeSys.Compiler35220
{
	// Token: 0x02000004 RID: 4
	public static class APEnvironmentFacade
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000003 RID: 3 RVA: 0x00002060 File Offset: 0x00000260
		// (set) Token: 0x06000004 RID: 4 RVA: 0x00002078 File Offset: 0x00000278
		public static IAPEnvironmentFacade Instance
		{
			get
			{
				if (APEnvironmentFacade.\u0001 == null)
				{
					APEnvironmentFacade.\u0001 = new APEnvironmentFacadeDesktop();
				}
				return APEnvironmentFacade.\u0001;
			}
			set
			{
				APEnvironmentFacade.\u0001 = value;
			}
		}

		// Token: 0x04000001 RID: 1
		private static IAPEnvironmentFacade \u0001;
	}
}
