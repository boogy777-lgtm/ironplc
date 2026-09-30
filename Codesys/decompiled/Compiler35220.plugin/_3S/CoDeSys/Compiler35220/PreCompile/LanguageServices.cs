using System;
using CODESYS.Parser;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000161 RID: 353
	public static class LanguageServices
	{
		// Token: 0x1700057F RID: 1407
		// (get) Token: 0x0600183F RID: 6207 RVA: 0x0004BC74 File Offset: 0x00049E74
		public static IParserService ParserService
		{
			get
			{
				return APEnvironmentFacade.Instance.ParserService;
			}
		}

		// Token: 0x17000580 RID: 1408
		// (get) Token: 0x06001840 RID: 6208 RVA: 0x0004BC80 File Offset: 0x00049E80
		public static IScannerService ScannerService
		{
			get
			{
				return APEnvironmentFacade.Instance.ScannerService;
			}
		}

		// Token: 0x06001841 RID: 6209 RVA: 0x0004BC8C File Offset: 0x00049E8C
		public static IScannerService GetScannerService(Version version)
		{
			return APEnvironmentFacade.Instance.GetScannerService(version);
		}
	}
}
