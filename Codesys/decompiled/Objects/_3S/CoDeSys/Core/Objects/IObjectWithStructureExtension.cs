using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200014F RID: 335
	[ReleasedInterface]
	public interface IObjectWithStructureExtension
	{
		// Token: 0x06000507 RID: 1287
		Guid GetDifferingParentGuid(Guid selectedParentGuid);
	}
}
