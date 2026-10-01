using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000116 RID: 278
	[ReleasedInterface]
	public interface IPSMove : IPSChange
	{
		// Token: 0x1700018C RID: 396
		// (get) Token: 0x06000437 RID: 1079
		IPSNode OldParentNode { get; }

		// Token: 0x1700018D RID: 397
		// (get) Token: 0x06000438 RID: 1080
		int OldIndex { get; }

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000439 RID: 1081
		IPSNode NewParentNode { get; }

		// Token: 0x1700018F RID: 399
		// (get) Token: 0x0600043A RID: 1082
		int NewIndex { get; }
	}
}
