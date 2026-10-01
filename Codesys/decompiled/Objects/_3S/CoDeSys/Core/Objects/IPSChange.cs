using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000113 RID: 275
	[ReleasedInterface]
	public interface IPSChange
	{
		// Token: 0x17000189 RID: 393
		// (get) Token: 0x06000434 RID: 1076
		IPSNode AffectedNode { get; }

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x06000435 RID: 1077
		PSChangeAction Action { get; }
	}
}
