using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200011C RID: 284
	[ReleasedInterface]
	public interface IPSObjectStreamEntry
	{
		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000465 RID: 1125
		bool IsRoot { get; }

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000466 RID: 1126
		IMetaObject MetaObject { get; }

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000467 RID: 1127
		IObject Object { get; }

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000468 RID: 1128
		Guid ParentSVNodeGuid { get; }

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000469 RID: 1129
		string[] Path { get; }

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x0600046A RID: 1130
		int ChildIndex { get; }
	}
}
