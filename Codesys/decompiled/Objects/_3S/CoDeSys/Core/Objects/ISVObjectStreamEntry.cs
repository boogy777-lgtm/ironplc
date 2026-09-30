using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200015A RID: 346
	[ReleasedInterface]
	public interface ISVObjectStreamEntry
	{
		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06000538 RID: 1336
		bool IsRoot { get; }

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06000539 RID: 1337
		IMetaObject MetaObject { get; }

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x0600053A RID: 1338
		IObject Object { get; }

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x0600053B RID: 1339
		Guid ParentSVNodeGuid { get; }

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x0600053C RID: 1340
		string[] Path { get; }
	}
}
