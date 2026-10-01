using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000105 RID: 261
	[ReleasedInterface]
	public interface IProjectStructure
	{
		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000405 RID: 1029
		int ProjectHandle { get; }

		// Token: 0x06000406 RID: 1030
		IPSNode[] GetNodes();

		// Token: 0x06000407 RID: 1031
		IPSNode FindNode(Guid objectGuid, bool bRecursive);

		// Token: 0x06000408 RID: 1032
		IPSNode AddNode(Guid objectGuid, IObject obj, string stName);

		// Token: 0x06000409 RID: 1033
		IPSNode AddNodeUnchecked(Guid objectGuid, IObject obj, string stName);

		// Token: 0x0600040A RID: 1034
		string GetUniqueName(Guid namespaceGuid, string stBaseName, out Guid conflictingObjectGuid);

		// Token: 0x0600040B RID: 1035
		IPSNode[] GetAllNodes();

		// Token: 0x0600040C RID: 1036
		void RemoveNodes(IPSNode[] nodes);

		// Token: 0x0600040D RID: 1037
		void CopyToStream(IPSNode[] nodes, Stream stream, bool bRecursive);

		// Token: 0x0600040E RID: 1038
		void CopyToStream(IPSNode[] nodes, Stream stream, Guid archiveWriterGuid, bool bRecursive, EventHandler<PSNodeEventArgs> progressHandler);

		// Token: 0x0600040F RID: 1039
		bool CanPasteFromStream(Stream stream);

		// Token: 0x06000410 RID: 1040
		bool CanPasteFromStream(Stream stream, Guid archiveReaderGuid);

		// Token: 0x06000411 RID: 1041
		void PasteFromStream(Stream stream, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler);

		// Token: 0x06000412 RID: 1042
		void PasteFromStream(Stream stream, Guid archiveReaderGuid, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler, EventHandler<PSPasteSkipEventArgs> skipHandler);

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x06000413 RID: 1043
		// (remove) Token: 0x06000414 RID: 1044
		event EventHandler<PSChangedEventArgs> Changed;
	}
}
