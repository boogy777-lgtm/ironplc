using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000118 RID: 280
	[ReleasedInterface]
	public interface IPSNode2 : IPSNode, IComparable<IPSNode>, IComparable
	{
		// Token: 0x06000458 RID: 1112
		bool CanPasteFromStream(IPSObjectStream objectStream);

		// Token: 0x06000459 RID: 1113
		void PasteFromStream(IPSObjectStream objectStream, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler, EventHandler<PSPasteSkipEventArgs> skipHandler);
	}
}
