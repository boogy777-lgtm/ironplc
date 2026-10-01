using System;
using System.Collections.Generic;
using System.Linq;
using CODESYS.Parser;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000AA RID: 170
	[TypeGuid("{dddddddd-beaa-46f0-b075-34396ae02add}")]
	[SystemInterface("_3S.CoDeSys.LanguageModelManager.InternalInterfaces._IScannerParserProvider")]
	public class ScannerParserProvider : _IScannerParserProvider2, _IScannerParserProvider
	{
		// Token: 0x06000A3B RID: 2619 RVA: 0x00017337 File Offset: 0x00016337
		internal void Reset()
		{
			this._parserToUse = null;
			this._scannerToUse = null;
		}

		// Token: 0x06000A3C RID: 2620 RVA: 0x00017347 File Offset: 0x00016347
		public IScannerService ScannerServiceToUseInternal()
		{
			if (this._scannerToUse == null)
			{
				this._scannerToUse = this.DetermineLanguageVersionDependentServiceToUse<IScannerService>(APEnvironmentFacade.Instance.GetAllScannerServices());
			}
			return this._scannerToUse;
		}

		// Token: 0x06000A3D RID: 2621 RVA: 0x0001736D File Offset: 0x0001636D
		public IParserService ParserServiceToUseInternal()
		{
			if (this._parserToUse == null)
			{
				this._parserToUse = this.DetermineLanguageVersionDependentServiceToUse<IParserService>(APEnvironmentFacade.Instance.GetAllParserServices());
			}
			return this._parserToUse;
		}

		// Token: 0x06000A3E RID: 2622 RVA: 0x00017393 File Offset: 0x00016393
		public IScannerService GetScannerService(Version version)
		{
			return this.DetermineLanguageVersionDependentServiceToUse<IScannerService>(APEnvironmentFacade.Instance.GetAllScannerServices(), version);
		}

		// Token: 0x06000A3F RID: 2623 RVA: 0x000173A6 File Offset: 0x000163A6
		public IParserService GetParserService(Version version)
		{
			return this.DetermineLanguageVersionDependentServiceToUse<IParserService>(APEnvironmentFacade.Instance.GetAllParserServices(), version);
		}

		// Token: 0x06000A40 RID: 2624 RVA: 0x000173BC File Offset: 0x000163BC
		private T DetermineLanguageVersionDependentServiceToUse<T>(IEnumerable<T> allLanguageVersionDependentServices) where T : ILanguageVersionDependentService
		{
			Version compilerversion = APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUseInternal();
			return this.DetermineLanguageVersionDependentServiceToUse<T>(allLanguageVersionDependentServices, compilerversion);
		}

		// Token: 0x06000A41 RID: 2625 RVA: 0x000173E4 File Offset: 0x000163E4
		private T DetermineLanguageVersionDependentServiceToUse<T>(IEnumerable<T> allLanguageVersionDependentServices, Version compilerversion) where T : ILanguageVersionDependentService
		{
			IEnumerable<T> source = from ps in allLanguageVersionDependentServices
			orderby ps.LanguageVersion
			select ps;
			T t = (from ps in source
			where ps.LanguageVersion <= compilerversion
			select ps).LastOrDefault<T>();
			if (t != null)
			{
				return t;
			}
			return source.FirstOrDefault<T>();
		}

		// Token: 0x0400017B RID: 379
		private IParserService _parserToUse;

		// Token: 0x0400017C RID: 380
		private IScannerService _scannerToUse;
	}
}
