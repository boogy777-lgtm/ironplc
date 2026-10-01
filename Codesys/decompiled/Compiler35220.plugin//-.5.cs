using System;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0082
{
	// Token: 0x02000167 RID: 359
	internal sealed class \u0005 : IPragmaScannerFactory
	{
		// Token: 0x0600189D RID: 6301 RVA: 0x0004C63C File Offset: 0x0004A83C
		private \u0005()
		{
		}

		// Token: 0x1700058B RID: 1419
		// (get) Token: 0x0600189E RID: 6302 RVA: 0x0004C644 File Offset: 0x0004A844
		public static IPragmaScannerFactory Singleton
		{
			get
			{
				return \u0005.\u0001;
			}
		}

		// Token: 0x0600189F RID: 6303 RVA: 0x0004C64C File Offset: 0x0004A84C
		public IPragmaScanner \u0001(string \u0002)
		{
			return APEnvironmentFacade.Instance.ScannerService.CreatePragmaScanner(this.\u0001(\u0002));
		}

		// Token: 0x060018A0 RID: 6304 RVA: 0x0004C664 File Offset: 0x0004A864
		public IPragmaScanner \u0001(_IScanner5 \u0002)
		{
			return APEnvironmentFacade.Instance.ScannerService.CreatePragmaScanner(\u0002);
		}

		// Token: 0x060018A1 RID: 6305 RVA: 0x0004C678 File Offset: 0x0004A878
		private IScanner9 \u0001(string \u0002)
		{
			_IScanner5 iscanner = Scanner.\u0001();
			iscanner.Initialize(\u0002);
			iscanner.AllowMultipleUnderlines = true;
			return iscanner;
		}

		// Token: 0x04000457 RID: 1111
		private static readonly \u0005 \u0001 = new \u0005();
	}
}
