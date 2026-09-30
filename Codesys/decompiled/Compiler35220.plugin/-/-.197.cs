using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0011
{
	// Token: 0x02000232 RID: 562
	internal sealed class \u0008 : IAccessMode
	{
		// Token: 0x0600253F RID: 9535 RVA: 0x00081B3C File Offset: 0x0007FD3C
		public bool \u0001(AccessModeFlags \u0002)
		{
			return (this.\u0001 & \u0002) > AccessModeFlags.Unknown;
		}

		// Token: 0x06002540 RID: 9536 RVA: 0x00081B4C File Offset: 0x0007FD4C
		public bool \u0002(AccessModeFlags \u0002)
		{
			return (this.\u0001 & \u0002) == \u0002;
		}

		// Token: 0x06002541 RID: 9537 RVA: 0x00081B5C File Offset: 0x0007FD5C
		public void \u0001(AccessModeFlags \u0002, bool \u0003)
		{
			if (\u0003)
			{
				this.\u0001 |= \u0002;
				return;
			}
			this.\u0001 &= ~\u0002;
		}

		// Token: 0x06002542 RID: 9538 RVA: 0x00081B80 File Offset: 0x0007FD80
		internal void \u0001()
		{
			this.\u0001 = AccessModeFlags.Unknown;
		}

		// Token: 0x06002543 RID: 9539 RVA: 0x00081B8C File Offset: 0x0007FD8C
		public IAccessMode \u0001()
		{
			return new \u0008
			{
				\u0001 = this.\u0001
			};
		}

		// Token: 0x040006AB RID: 1707
		private AccessModeFlags \u0001;
	}
}
