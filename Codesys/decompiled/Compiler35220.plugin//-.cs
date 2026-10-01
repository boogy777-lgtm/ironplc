using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0083
{
	// Token: 0x02000109 RID: 265
	internal sealed class \u0001 : IAccessInfo2, IAccessInfo
	{
		// Token: 0x060013C4 RID: 5060 RVA: 0x00038DBC File Offset: 0x00036FBC
		public \u0001(ISourcePosition \u001E\u0002, AccessFlag \u007F\u0002, Guid \u0084\u0002, Guid \u0086\u0002)
		{
			this.\u0001 = \u001E\u0002;
			this.\u0001 = \u007F\u0002;
			this.\u0001 = \u0084\u0002;
			this.\u0002 = \u0086\u0002;
		}

		// Token: 0x060013C5 RID: 5061 RVA: 0x00038DE4 File Offset: 0x00036FE4
		public bool \u0001(AccessFlag \u0002)
		{
			return (this.\u0001 & \u0002) == \u0002;
		}

		// Token: 0x170004D9 RID: 1241
		// (get) Token: 0x060013C6 RID: 5062 RVA: 0x00038DF4 File Offset: 0x00036FF4
		public ISourcePosition Position
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170004DA RID: 1242
		// (get) Token: 0x060013C7 RID: 5063 RVA: 0x00038DFC File Offset: 0x00036FFC
		public AccessFlag Access
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170004DB RID: 1243
		// (get) Token: 0x060013C8 RID: 5064 RVA: 0x00038E04 File Offset: 0x00037004
		public Guid ApplicationGuid
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170004DC RID: 1244
		// (get) Token: 0x060013C9 RID: 5065 RVA: 0x00038E0C File Offset: 0x0003700C
		public Guid MessageGuid
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x0400034E RID: 846
		private ISourcePosition \u0001;

		// Token: 0x0400034F RID: 847
		private AccessFlag \u0001;

		// Token: 0x04000350 RID: 848
		private Guid \u0001;

		// Token: 0x04000351 RID: 849
		private Guid \u0002;
	}
}
