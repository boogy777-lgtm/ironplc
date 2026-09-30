using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F8 RID: 248
	[ReleasedInterface]
	public interface IParentInfo
	{
		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060003E7 RID: 999
		Guid ObjectGuid { get; }

		// Token: 0x060003E8 RID: 1000
		Guid GetFolderObjectGuid(Guid structuredViewGuid);

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x060003E9 RID: 1001
		Guid[] StructuredViewGuids { get; }
	}
}
