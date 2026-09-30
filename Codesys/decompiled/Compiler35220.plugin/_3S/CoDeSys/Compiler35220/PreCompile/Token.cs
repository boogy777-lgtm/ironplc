using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000168 RID: 360
	public static class Token
	{
		// Token: 0x1700058C RID: 1420
		// (get) Token: 0x060018A3 RID: 6307 RVA: 0x0004C69C File Offset: 0x0004A89C
		public static _IToken Empty
		{
			get
			{
				return APEnvironmentFacade.Instance.ScannerService.TokenFactory.CreateEmptyToken();
			}
		}
	}
}
