using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0004
{
	// Token: 0x020001E3 RID: 483
	internal class \u0008 : IPOUInfoStruct, ICompiledElementInfoStruct
	{
		// Token: 0x17000637 RID: 1591
		// (get) Token: 0x06002153 RID: 8531 RVA: 0x00071CF8 File Offset: 0x0006FEF8
		public uint CRCCode
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000638 RID: 1592
		// (get) Token: 0x06002154 RID: 8532 RVA: 0x00071D00 File Offset: 0x0006FF00
		public uint CRCInterface
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x17000639 RID: 1593
		// (get) Token: 0x06002155 RID: 8533 RVA: 0x00071D08 File Offset: 0x0006FF08
		public string Name
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x1700063A RID: 1594
		// (get) Token: 0x06002156 RID: 8534 RVA: 0x00071D10 File Offset: 0x0006FF10
		public ushort AreaCodeLocation
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x1700063B RID: 1595
		// (get) Token: 0x06002157 RID: 8535 RVA: 0x00071D18 File Offset: 0x0006FF18
		public ushort AreaFPPointerLocation
		{
			get
			{
				return this.\u0002;
			}
		}

		// Token: 0x1700063C RID: 1596
		// (get) Token: 0x06002158 RID: 8536 RVA: 0x00071D20 File Offset: 0x0006FF20
		public uint OffsetCodeLocation
		{
			get
			{
				return this.\u0004;
			}
		}

		// Token: 0x1700063D RID: 1597
		// (get) Token: 0x06002159 RID: 8537 RVA: 0x00071D28 File Offset: 0x0006FF28
		public uint OffsetFPPointerLocation
		{
			get
			{
				return this.\u0005;
			}
		}

		// Token: 0x0400058B RID: 1419
		public uint \u0001;

		// Token: 0x0400058C RID: 1420
		public uint \u0002;

		// Token: 0x0400058D RID: 1421
		public uint \u0003;

		// Token: 0x0400058E RID: 1422
		public ushort \u0001;

		// Token: 0x0400058F RID: 1423
		public ushort \u0002;

		// Token: 0x04000590 RID: 1424
		public uint \u0004;

		// Token: 0x04000591 RID: 1425
		public uint \u0005;

		// Token: 0x04000592 RID: 1426
		public string \u0001;
	}
}
