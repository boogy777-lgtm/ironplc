using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000102 RID: 258
	[ReleasedInterface]
	public interface IProjectSource : IDisposable
	{
		// Token: 0x1700017E RID: 382
		// (get) Token: 0x060003F8 RID: 1016
		ProjectSourceFlags Flags { get; }

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x060003F9 RID: 1017
		ICollection<IProjectSourceNode> RootObjects { get; }

		// Token: 0x17000180 RID: 384
		IProjectSourceNode this[Guid guid]
		{
			get;
		}

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x060003FB RID: 1019
		// (remove) Token: 0x060003FC RID: 1020
		event EventHandler<ObjectTakeOverRequiredEventArgs> ObjectTakeOverRequired;

		// Token: 0x060003FD RID: 1021
		bool PushOneUntakenObject();

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x060003FE RID: 1022
		bool HasUntakenObjects { get; }
	}
}
