using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000128 RID: 296
	[ReleasedInterface]
	public interface IPastedObject
	{
		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000492 RID: 1170
		Guid ObjectGuid { get; }

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06000493 RID: 1171
		Guid OldObjectGuid { get; }

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x06000494 RID: 1172
		// (set) Token: 0x06000495 RID: 1173
		Guid ParentSVNodeGuid { get; set; }

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x06000496 RID: 1174
		Guid OldParentSVNodeGuid { get; }

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x06000497 RID: 1175
		// (set) Token: 0x06000498 RID: 1176
		int PastePosition { get; set; }

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06000499 RID: 1177
		// (set) Token: 0x0600049A RID: 1178
		IObject Object { get; set; }

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x0600049B RID: 1179
		string Name { get; }

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x0600049C RID: 1180
		IObjectProperty[] Properties { get; }
	}
}
