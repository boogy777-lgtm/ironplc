using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000107 RID: 263
	[ReleasedInterface]
	public interface IProjectStructure3 : IProjectStructure2, IProjectStructure
	{
		// Token: 0x0600041B RID: 1051
		void CopyToStream(IPSNode[] nodes, Stream stream, Guid archiveWriterGuid, Profile profile, string stProfileName, bool bRecursive, EventHandler<PSNodeEventArgs> progressHandler, IArchiveReporter archiveReporter);
	}
}
