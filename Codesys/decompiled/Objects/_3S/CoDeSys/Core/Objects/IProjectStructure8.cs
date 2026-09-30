using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200010C RID: 268
	[ReleasedInterface]
	public interface IProjectStructure8 : IProjectStructure7, IProjectStructure6, IProjectStructure5, IProjectStructure4, IProjectStructure3, IProjectStructure2, IProjectStructure
	{
		// Token: 0x06000428 RID: 1064
		void PasteFromStream(IPSObjectStream objectStream, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler, EventHandler<PSPasteSkipEventArgs> skipHandler, PasteFolderHandling pasteFolderHandling);

		// Token: 0x06000429 RID: 1065
		void PasteFromStream(Stream stream, Guid archiveReaderGuid, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler, EventHandler<PSPasteSkipEventArgs> skipHandler, PasteFolderHandling pasteFolderHandling);
	}
}
