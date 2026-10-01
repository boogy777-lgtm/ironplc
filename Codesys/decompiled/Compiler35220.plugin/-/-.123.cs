using System;
using System.IO;
using System.Reflection;
using \u001D;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0014
{
	// Token: 0x0200016F RID: 367
	internal static class \u0007
	{
		// Token: 0x060018E5 RID: 6373 RVA: 0x0004D918 File Offset: 0x0004BB18
		internal static void \u0001(_ILanguageModelManagerConsolidated \u0002)
		{
			\u0007.\u0001();
			\u0007.\u0002(\u0002);
		}

		// Token: 0x060018E6 RID: 6374 RVA: 0x0004D928 File Offset: 0x0004BB28
		private static void \u0002(_ILanguageModelManagerConsolidated \u0002)
		{
			Assembly assembly = Assembly.GetAssembly(\u0002.GetType());
			\u0004 u = new \u0004();
			\u0007.\u0001("_3S.CoDeSys.LanguageModelManager.Resources.sysmemcopy.xml", u, assembly, \u0002);
			\u0007.\u0001("_3S.CoDeSys.LanguageModelManager.Resources.syslibs.xml", u, assembly, \u0002);
		}

		// Token: 0x060018E7 RID: 6375 RVA: 0x0004D964 File Offset: 0x0004BB64
		private static void \u0001()
		{
			APEnvironmentFacade.Instance.LMServiceProvider.LanguageModelProviderService.RemoveLanguageModelOfObject(APEnvironmentFacade.Instance.PrimaryProjectHandle, \u0007.\u0001);
		}

		// Token: 0x060018E8 RID: 6376 RVA: 0x0004D98C File Offset: 0x0004BB8C
		private static void \u0001(string \u0002, \u0004 \u0003, Assembly \u0004, _ILanguageModelManagerConsolidated \u0005)
		{
			using (Stream manifestResourceStream = \u0004.GetManifestResourceStream(\u0002))
			{
				using (StreamReader streamReader = new StreamReader(manifestResourceStream))
				{
					string u = streamReader.ReadToEnd();
					\u0003.\u0002(\u0005, u, false, string.Empty, Guid.Empty, \u0007.\u0001, string.Empty, false, false, \u0007.\u0001, null);
				}
			}
		}

		// Token: 0x04000466 RID: 1126
		private static readonly Guid \u0001 = Guid.Parse("{BFFFA1C4-8023-4494-8630-FFEE7D325559}");

		// Token: 0x04000467 RID: 1127
		private static readonly SignatureFlag \u0001 = SignatureFlag.SuperGlobal | SignatureFlag.TopLevel | SignatureFlag.SimulationExternal;
	}
}
