using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x0200015C RID: 348
	public class ScannerOptionsService : IScannerOptionsService
	{
		// Token: 0x1700057A RID: 1402
		// (get) Token: 0x06001826 RID: 6182 RVA: 0x0004B2B0 File Offset: 0x000494B0
		public static ScannerOptionsService Singleton { get; } = new ScannerOptionsService();

		// Token: 0x06001827 RID: 6183 RVA: 0x0004B2B8 File Offset: 0x000494B8
		public void GetScanningOptions(out bool bUnicodeIdentifiers, out bool bSupportNonCompliantIdentifiers)
		{
			bUnicodeIdentifiers = false;
			bSupportNonCompliantIdentifiers = true;
			if (APEnvironmentFacade.Instance.InjectionCompleted)
			{
				bUnicodeIdentifiers = APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers;
				if (APEnvironmentFacade.Instance.OEMCustomization.HasValue("LanguageModelManager", "NonCompliantIdentifiers"))
				{
					bSupportNonCompliantIdentifiers = APEnvironmentFacade.Instance.OEMCustomization.GetBoolValue("LanguageModelManager", "NonCompliantIdentifiers");
				}
				bSupportNonCompliantIdentifiers &= (APEnvironmentFacade.Instance.CompilerVersionToUseInternal() >= new Version(3, 5, 18, 0));
			}
		}

		// Token: 0x04000446 RID: 1094
		[CompilerGenerated]
		private static readonly ScannerOptionsService \u0001;
	}
}
