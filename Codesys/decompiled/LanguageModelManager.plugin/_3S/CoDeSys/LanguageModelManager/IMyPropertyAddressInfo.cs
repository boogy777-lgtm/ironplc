using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000EC RID: 236
	internal interface IMyPropertyAddressInfo : IAddressInfo, IPropertyAdressInfoWithOffset
	{
		// Token: 0x17000501 RID: 1281
		// (get) Token: 0x0600118A RID: 4490
		// (set) Token: 0x0600118B RID: 4491
		int AreaProperty { get; set; }

		// Token: 0x17000502 RID: 1282
		// (get) Token: 0x0600118C RID: 4492
		// (set) Token: 0x0600118D RID: 4493
		int OffsetGet { get; set; }

		// Token: 0x17000503 RID: 1283
		// (get) Token: 0x0600118E RID: 4494
		// (set) Token: 0x0600118F RID: 4495
		int OffsetSet { get; set; }

		// Token: 0x17000504 RID: 1284
		// (get) Token: 0x06001190 RID: 4496
		// (set) Token: 0x06001191 RID: 4497
		int VFTableOffsetGet { get; set; }

		// Token: 0x17000505 RID: 1285
		// (get) Token: 0x06001192 RID: 4498
		// (set) Token: 0x06001193 RID: 4499
		int VFTableOffsetSet { get; set; }

		// Token: 0x17000506 RID: 1286
		// (get) Token: 0x06001194 RID: 4500
		// (set) Token: 0x06001195 RID: 4501
		bool IsReferenceType { get; set; }
	}
}
