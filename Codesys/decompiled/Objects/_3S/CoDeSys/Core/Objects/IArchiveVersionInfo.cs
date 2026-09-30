using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200013D RID: 317
	[ReleasedInterface]
	public interface IArchiveVersionInfo
	{
		// Token: 0x170001BE RID: 446
		// (get) Token: 0x060004DC RID: 1244
		Profile Profile { get; }

		// Token: 0x060004DD RID: 1245
		Version GetTargetVersion(object obj);

		// Token: 0x060004DE RID: 1246
		Version GetCurrentVersion(object obj);
	}
}
