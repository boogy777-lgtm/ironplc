using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200012E RID: 302
	[ReleasedInterface]
	public interface IStructuredView4 : IStructuredView3, IStructuredView2, IStructuredView
	{
		// Token: 0x060004C2 RID: 1218
		void CopyToStream(ISVNode[] nodes, Stream stream, Guid archiveWriterGuid, Profile profile, string stProfileName, bool bRecursive, StructuredViewCopyNodeEventHandler progressHandler, IArchiveReporter archiveReporter);

		// Token: 0x060004C3 RID: 1219
		ISVObjectStream CreateObjectStream();
	}
}
