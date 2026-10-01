using System;
using System.Collections.Generic;
using \u0018;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u001E
{
	// Token: 0x02000385 RID: 901
	internal sealed class \u0016 : \u001E
	{
		// Token: 0x06003498 RID: 13464 RVA: 0x000CF758 File Offset: 0x000CD958
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			\u0002.ComconNew = \u0002.ComconOld.Duplicate();
			if (\u0002.ComconNew.DataManager._MemorySettings.AdditionalAreas)
			{
				IEnumerable<_IArea> u = Locator.\u0001(\u0002.ComconOld, \u0002.ComconNew.DataManager._MemorySettings, \u0002.ComconNew.DataManager._MemorySettings._Areas);
				MemoryCompiler.\u0001(\u0002.ComconNew.DataManager, \u0002.ComconNew.DataManager._MemorySettings, u);
			}
			return true;
		}
	}
}
