using System;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000233 RID: 563
	[Flags]
	internal enum GreenVariableProperies
	{
		// Token: 0x04000710 RID: 1808
		None = 0,
		// Token: 0x04000711 RID: 1809
		Hide = 1,
		// Token: 0x04000712 RID: 1810
		Property = 2,
		// Token: 0x04000713 RID: 1811
		NoInit = 4,
		// Token: 0x04000714 RID: 1812
		NoPrecompileChecks = 8,
		// Token: 0x04000715 RID: 1813
		BlobInitConst = 16,
		// Token: 0x04000716 RID: 1814
		InitOnOnlChange = 32,
		// Token: 0x04000717 RID: 1815
		SuppressWarning0 = 64
	}
}
