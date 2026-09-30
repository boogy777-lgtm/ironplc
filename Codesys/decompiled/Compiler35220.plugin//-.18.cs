using System;
using System.Collections.Generic;
using \u0018;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0083;
using \u0084;

namespace \u0080
{
	// Token: 0x02000378 RID: 888
	internal sealed class \u0017 : \u001E
	{
		// Token: 0x0600344B RID: 13387 RVA: 0x000CDAB8 File Offset: 0x000CBCB8
		private static bool \u0001()
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			return oemcustomization != null && oemcustomization.HasValue("LanguageModelManager", "SuppressCheckForChangedLateLm") && oemcustomization.GetBoolValue("LanguageModelManager", "SuppressCheckForChangedLateLm");
		}

		// Token: 0x0600344C RID: 13388 RVA: 0x000CDAF8 File Offset: 0x000CBCF8
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			if (\u0017.\u0001())
			{
				return true;
			}
			_ILanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			languageModelMgr[\u0002.ApplicationGuid] = \u0002.ComconNew;
			ILanguageModelList languageModelList = \u0019.\u0003.Builder.CreateLanguageModelList();
			List<ILanguageModel> list = new List<ILanguageModel>();
			AddLanguageModelEventArgs3 e = new AddLanguageModelEventArgs3(\u0002.ApplicationGuid, languageModelList, list, \u0019.\u0003.Builder, true);
			languageModelMgr.OnAddLateLanguageModel(e);
			return \u001D.\u0004.\u0001(\u0002.ComconNew, languageModelList, \u0002.ComconOld, true, false) && \u0083.\u0007.\u0001(\u0002.ComconNew, list, \u0002.ComconOld, true, false);
		}
	}
}
