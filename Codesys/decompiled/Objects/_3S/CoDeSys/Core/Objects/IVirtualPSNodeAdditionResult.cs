using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000121 RID: 289
	[ReleasedInterface]
	public interface IVirtualPSNodeAdditionResult<TVirtualPSNode> where TVirtualPSNode : IVirtualPSNode
	{
		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000474 RID: 1140
		TVirtualPSNode VirtualNode { get; }

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06000475 RID: 1141
		IPSNode RealNode { get; }

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000476 RID: 1142
		Exception ExceptionDuringAddition { get; }

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000477 RID: 1143
		ICollection<Exception> ExceptionsDuringEvents { get; }
	}
}
