using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Messaging.MessageDecorator
{
	// Token: 0x0200039E RID: 926
	[TypeGuid("{22222222-f124-4876-95ce-63a76ed82ddd}")]
	public class CodesysC541MessageDecoratorInfoProvider : IC541MessageDecoratorInfoProvider
	{
		// Token: 0x170008CB RID: 2251
		// (get) Token: 0x060035E3 RID: 13795 RVA: 0x000D775C File Offset: 0x000D595C
		public IEnumerable<Guid> PlugInGuids
		{
			get
			{
				return CodesysC541MessageDecoratorInfoProvider.\u0001.Keys;
			}
		}

		// Token: 0x060035E4 RID: 13796 RVA: 0x000D7768 File Offset: 0x000D5968
		public string GetProvidingAddOnName(Guid gdMemoryAllocationCallback)
		{
			string result;
			if (CodesysC541MessageDecoratorInfoProvider.\u0001.TryGetValue(gdMemoryAllocationCallback, out result))
			{
				return result;
			}
			return string.Empty;
		}

		// Token: 0x04000A78 RID: 2680
		private static Dictionary<Guid, string> \u0001 = new Dictionary<Guid, string>
		{
			{
				new Guid("{1CBEF7A1-908E-43D3-B3B6-6A7BC46461F6}"),
				\u0081.\u0001.MemoryAllocator_RuntimeExtension
			}
		};
	}
}
