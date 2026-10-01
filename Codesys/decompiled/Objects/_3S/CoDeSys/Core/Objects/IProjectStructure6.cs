using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200010A RID: 266
	[ReleasedInterface]
	public interface IProjectStructure6 : IProjectStructure5, IProjectStructure4, IProjectStructure3, IProjectStructure2, IProjectStructure
	{
		// Token: 0x06000424 RID: 1060
		IPSObjectStream CreateObjectStream();

		// Token: 0x06000425 RID: 1061
		bool CanPasteFromStream(IPSObjectStream objectStream);

		// Token: 0x06000426 RID: 1062
		void PasteFromStream(IPSObjectStream objectStream, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler, EventHandler<PSPasteSkipEventArgs> skipHandler);
	}
}
