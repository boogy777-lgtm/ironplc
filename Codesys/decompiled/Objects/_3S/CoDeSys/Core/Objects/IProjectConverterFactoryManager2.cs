using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000100 RID: 256
	[ReleasedInterface]
	public interface IProjectConverterFactoryManager2 : IProjectConverterFactoryManager
	{
		// Token: 0x1400001E RID: 30
		// (add) Token: 0x060003F3 RID: 1011
		// (remove) Token: 0x060003F4 RID: 1012
		event ProjectBeforeImportEventHandler BeforeProjectConversion;

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x060003F5 RID: 1013
		// (remove) Token: 0x060003F6 RID: 1014
		event ProjectAfterImportEventHandler AfterProjectConversion;
	}
}
