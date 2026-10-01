using System;
using _3S.CoDeSys.Core.Options;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000125 RID: 293
	internal static class LoadAndSaveOptionsHelper
	{
		// Token: 0x17000618 RID: 1560
		// (get) Token: 0x0600190E RID: 6414 RVA: 0x000489B9 File Offset: 0x000479B9
		// (set) Token: 0x0600190F RID: 6415 RVA: 0x000489EC File Offset: 0x000479EC
		public static bool EnableBackgroundLoading
		{
			get
			{
				return !LoadAndSaveOptionsHelper.OptionKey.HasValue(LoadAndSaveOptionsHelper.ENABLE_BACKGROUND_LOADING, typeof(bool)) || (bool)LoadAndSaveOptionsHelper.OptionKey[LoadAndSaveOptionsHelper.ENABLE_BACKGROUND_LOADING];
			}
			set
			{
				LoadAndSaveOptionsHelper.OptionKey[LoadAndSaveOptionsHelper.ENABLE_BACKGROUND_LOADING] = value;
			}
		}

		// Token: 0x17000619 RID: 1561
		// (get) Token: 0x06001910 RID: 6416 RVA: 0x00048A03 File Offset: 0x00047A03
		private static IOptionKey OptionKey
		{
			get
			{
				return APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.User, LoadAndSaveOptionsHelper.SUB_KEY);
			}
		}

		// Token: 0x0400052B RID: 1323
		private static readonly string ENABLE_BACKGROUND_LOADING = "EnableBackgroundLoading";

		// Token: 0x0400052C RID: 1324
		private static readonly string SUB_KEY = "{A45C4DEC-304A-491c-BB07-0413896AA152}";
	}
}
