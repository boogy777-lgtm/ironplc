using System;
using System.Collections.Generic;
using \u0004;
using _3S.CoDeSys.Compiler35220.Messaging.MessageDecorator;
using _3S.CoDeSys.Core.LanguageModel;
using \u0081;

namespace \u007F
{
	// Token: 0x0200039F RID: 927
	internal sealed class \u0012 : global::\u0004.\u0017
	{
		// Token: 0x060035E7 RID: 13799 RVA: 0x000D77B8 File Offset: 0x000D59B8
		private void \u0001()
		{
			foreach (IC541MessageDecoratorInfoProvider ic541MessageDecoratorInfoProvider in new IC541MessageDecoratorInfoProvider[]
			{
				new CodesysC541MessageDecoratorInfoProvider()
			})
			{
				foreach (Guid guid in ic541MessageDecoratorInfoProvider.PlugInGuids)
				{
					if (!\u007F.\u0012.\u0001.ContainsKey(guid))
					{
						string providingAddOnName = ic541MessageDecoratorInfoProvider.GetProvidingAddOnName(guid);
						if (!string.IsNullOrEmpty(providingAddOnName))
						{
							\u007F.\u0012.\u0001.Add(guid, providingAddOnName);
						}
					}
				}
			}
		}

		// Token: 0x060035E8 RID: 13800 RVA: 0x000D7868 File Offset: 0x000D5A68
		internal \u0012(Guid \u007F\u0006)
		{
			this.\u0001 = \u007F\u0006;
			this.\u0001();
		}

		// Token: 0x060035E9 RID: 13801 RVA: 0x000D7880 File Offset: 0x000D5A80
		public string \u0001(string \u0002)
		{
			string arg;
			if (\u007F.\u0012.\u0001.TryGetValue(this.\u0001, out arg))
			{
				return string.Format(\u0081.\u0001.Err_NoMemoryAllocationCallbackDetailed, arg);
			}
			return \u0002;
		}

		// Token: 0x04000A79 RID: 2681
		private static Dictionary<Guid, string> \u0001 = new Dictionary<Guid, string>();

		// Token: 0x04000A7A RID: 2682
		private readonly Guid \u0001;
	}
}
