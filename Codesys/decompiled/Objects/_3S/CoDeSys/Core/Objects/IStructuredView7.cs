using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000131 RID: 305
	[ReleasedInterface]
	public interface IStructuredView7 : IStructuredView6, IStructuredView5, IStructuredView4, IStructuredView3, IStructuredView2, IStructuredView
	{
		// Token: 0x060004C8 RID: 1224
		void PasteFromStream(ISVNode destNode, ISVObjectStream objectStream, StructuredViewPasteNodeEventHandler progressHandler, StructuredViewPasteConflictEventHandler conflictHandler, StructuredViewPasteSkipEventHandler skipHandler, PasteFolderHandling pasteFolderHandling);

		// Token: 0x060004C9 RID: 1225
		void PasteFromStream(ISVNode destNode, Stream stream, Guid archiveReaderGuid, StructuredViewPasteNodeEventHandler progressHandler, StructuredViewPasteConflictEventHandler conflictHandler, StructuredViewPasteSkipEventHandler skipHandler, PasteFolderHandling pasteFolderHandling);
	}
}
