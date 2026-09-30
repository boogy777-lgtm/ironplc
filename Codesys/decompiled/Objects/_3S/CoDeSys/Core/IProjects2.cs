using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000025 RID: 37
	[ReleasedInterface]
	public interface IProjects2 : IProjects
	{
		// Token: 0x14000009 RID: 9
		// (add) Token: 0x060000AC RID: 172
		// (remove) Token: 0x060000AD RID: 173
		event ProjectImportedEventHandler ProjectImported;
	}
}
