using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000104 RID: 260
	[ReleasedInterface]
	public interface IProjectSourceNode : IVirtualPSNode
	{
		// Token: 0x17000182 RID: 386
		// (get) Token: 0x060003FF RID: 1023
		IProjectSource ProjectSource { get; }

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000400 RID: 1024
		IProjectSourceNode Parent { get; }

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000401 RID: 1025
		Type[] EmbeddedObjectTypes { get; }

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000402 RID: 1026
		bool Accessed { get; }

		// Token: 0x06000403 RID: 1027
		void MarkAsTaken();

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000404 RID: 1028
		bool WasTaken { get; }
	}
}
