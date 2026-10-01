using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000122 RID: 290
	[ReleasedInterface]
	public interface IVirtualPSRoot<TVirtualPSNode> where TVirtualPSNode : IVirtualPSNode
	{
		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000478 RID: 1144
		IPSNode Parent { get; }

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000479 RID: 1145
		TVirtualPSNode Node { get; }
	}
}
