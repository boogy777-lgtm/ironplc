using System;
using \u0004;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u007F
{
	// Token: 0x020001E4 RID: 484
	internal sealed class \u0006 : \u0008, IPOUInfoStruct, ICompiledElementInfoStruct, IFBInfoStruct
	{
		// Token: 0x1700063E RID: 1598
		// (get) Token: 0x0600215B RID: 8539 RVA: 0x00071D38 File Offset: 0x0006FF38
		public uint CRCVFTable
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x1700063F RID: 1599
		// (get) Token: 0x0600215C RID: 8540 RVA: 0x00071D40 File Offset: 0x0006FF40
		public uint AreaVFTableLocation
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x17000640 RID: 1600
		// (get) Token: 0x0600215D RID: 8541 RVA: 0x00071D48 File Offset: 0x0006FF48
		public uint OffsetVFTableLocation
		{
			get
			{
				return this.\u0003;
			}
		}

		// Token: 0x04000593 RID: 1427
		public new uint \u0001;

		// Token: 0x04000594 RID: 1428
		public new uint \u0002;

		// Token: 0x04000595 RID: 1429
		public new uint \u0003;
	}
}
