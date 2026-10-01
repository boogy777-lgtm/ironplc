using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200011F RID: 287
	[ReleasedInterface]
	public interface IVirtualPSNode
	{
		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x0600046D RID: 1133
		Guid ObjectGuid { get; }

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x0600046E RID: 1134
		Type ObjectType { get; }

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x0600046F RID: 1135
		string Name { get; }

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000470 RID: 1136
		IEnumerable<IVirtualPSNode> ChildNodes { get; }

		// Token: 0x06000471 RID: 1137
		IObject GetObject();

		// Token: 0x06000472 RID: 1138
		IObjectProperty[] GetProperties();
	}
}
