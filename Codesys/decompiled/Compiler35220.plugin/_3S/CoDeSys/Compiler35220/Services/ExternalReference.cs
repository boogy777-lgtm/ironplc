using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000E3 RID: 227
	public class ExternalReference : IExternalReference2, IExternalReference
	{
		// Token: 0x06000FDB RID: 4059 RVA: 0x0002CC1C File Offset: 0x0002AE1C
		public ExternalReference(string stName, IDataLocation dataloc, ISignature2 sign, uint uiCrc)
		{
			this.\u0001 = stName;
			this.PointerLocation = dataloc;
			this.CRC = uiCrc;
			this.Signature = sign;
		}

		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x06000FDC RID: 4060 RVA: 0x0002CC44 File Offset: 0x0002AE44
		// (set) Token: 0x06000FDD RID: 4061 RVA: 0x0002CC54 File Offset: 0x0002AE54
		public string Name
		{
			get
			{
				return this.\u0001.ToUpperInvariant();
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x06000FDE RID: 4062 RVA: 0x0002CC60 File Offset: 0x0002AE60
		// (set) Token: 0x06000FDF RID: 4063 RVA: 0x0002CC68 File Offset: 0x0002AE68
		public IDataLocation PointerLocation { get; set; }

		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x06000FE0 RID: 4064 RVA: 0x0002CC74 File Offset: 0x0002AE74
		// (set) Token: 0x06000FE1 RID: 4065 RVA: 0x0002CC7C File Offset: 0x0002AE7C
		public ISignature2 Signature { get; set; }

		// Token: 0x17000480 RID: 1152
		// (get) Token: 0x06000FE2 RID: 4066 RVA: 0x0002CC88 File Offset: 0x0002AE88
		// (set) Token: 0x06000FE3 RID: 4067 RVA: 0x0002CC90 File Offset: 0x0002AE90
		public uint CRC { get; set; }

		// Token: 0x040002BD RID: 701
		private string \u0001;

		// Token: 0x040002BE RID: 702
		[CompilerGenerated]
		private IDataLocation \u0001;

		// Token: 0x040002BF RID: 703
		[CompilerGenerated]
		private ISignature2 \u0001;

		// Token: 0x040002C0 RID: 704
		[CompilerGenerated]
		private uint \u0001;
	}
}
