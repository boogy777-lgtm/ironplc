using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200012C RID: 300
	[ReleasedInterface]
	public interface IStructuredView2 : IStructuredView
	{
		// Token: 0x060004BC RID: 1212
		void CopyToStream(ISVNode[] nodes, Stream stream, Guid archiveWriterGuid, bool bRecursive, StructuredViewCopyNodeEventHandler progressHandler);

		// Token: 0x060004BD RID: 1213
		bool CanPasteFromStream(ISVNode destNode, Stream stream, Guid archiveReaderGuid);

		// Token: 0x060004BE RID: 1214
		void PasteFromStream(ISVNode destNode, Stream stream, Guid archiveReaderGuid, StructuredViewPasteNodeEventHandler progressHandler, StructuredViewPasteConflictEventHandler conflictHandler, StructuredViewPasteSkipEventHandler skipHandler);

		// Token: 0x060004BF RID: 1215
		void MergeFromStream(Stream stream, Guid archiveReaderGuid, StructuredViewPasteNodeEventHandler progressHandler, StructuredViewPasteConflictEventHandler conflictHandler, StructuredViewPasteSkipEventHandler skipHandler);
	}
}
