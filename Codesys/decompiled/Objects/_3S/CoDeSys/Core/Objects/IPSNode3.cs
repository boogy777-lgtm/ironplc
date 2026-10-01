using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000119 RID: 281
	[ReleasedInterface]
	public interface IPSNode3 : IPSNode2, IPSNode, IComparable<IPSNode>, IComparable
	{
		// Token: 0x0600045A RID: 1114
		void PasteFromStream(IPSObjectStream objectStream, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler, EventHandler<PSPasteSkipEventArgs> skipHandler, PasteFolderHandling pasteFolderHandling);

		// Token: 0x0600045B RID: 1115
		void PasteFromStream(Stream stream, Guid archiveReaderGuid, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler, EventHandler<PSPasteSkipEventArgs> skipHandler, PasteFolderHandling pasteFolderHandling);
	}
}
