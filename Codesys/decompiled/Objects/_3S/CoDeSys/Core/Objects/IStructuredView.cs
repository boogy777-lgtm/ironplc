using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200012B RID: 299
	[ReleasedInterface]
	public interface IStructuredView
	{
		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x060004A2 RID: 1186
		ISVNode[] Children { get; }

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x060004A3 RID: 1187
		int ChildCount { get; }

		// Token: 0x060004A4 RID: 1188
		ISVNode GetChild(int nIndex);

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x060004A5 RID: 1189
		Guid StructuredViewGuid { get; }

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x060004A6 RID: 1190
		int ProjectHandle { get; }

		// Token: 0x060004A7 RID: 1191
		ISVNode GetNode(Guid objectGuid);

		// Token: 0x060004A8 RID: 1192
		void AddObject(Guid objectGuid, IObject obj, string stName);

		// Token: 0x060004A9 RID: 1193
		void Remove(ISVNode[] nodes);

		// Token: 0x060004AA RID: 1194
		void CopyToStream(ISVNode[] nodes, Stream stream, bool bRecursive);

		// Token: 0x060004AB RID: 1195
		bool CanPasteFromStream(ISVNode destNode, Stream stream);

		// Token: 0x060004AC RID: 1196
		void PasteFromStream(ISVNode destNode, Stream stream, StructuredViewPasteNodeEventHandler progressHandler, StructuredViewPasteConflictEventHandler conflictHandler);

		// Token: 0x060004AD RID: 1197
		void MergeFromStream(Stream stream, StructuredViewPasteNodeEventHandler progressHandler, StructuredViewPasteConflictEventHandler conflictHandler);

		// Token: 0x1400002F RID: 47
		// (add) Token: 0x060004AE RID: 1198
		// (remove) Token: 0x060004AF RID: 1199
		event SVNodeEventHandler NodeAdded;

		// Token: 0x14000030 RID: 48
		// (add) Token: 0x060004B0 RID: 1200
		// (remove) Token: 0x060004B1 RID: 1201
		event SVNodeEventHandler NodeRemoved;

		// Token: 0x14000031 RID: 49
		// (add) Token: 0x060004B2 RID: 1202
		// (remove) Token: 0x060004B3 RID: 1203
		event SVNodeEventHandler NodeLoaded;

		// Token: 0x14000032 RID: 50
		// (add) Token: 0x060004B4 RID: 1204
		// (remove) Token: 0x060004B5 RID: 1205
		event SVNodeModifiedEventHandler NodeModified;

		// Token: 0x14000033 RID: 51
		// (add) Token: 0x060004B6 RID: 1206
		// (remove) Token: 0x060004B7 RID: 1207
		event SVNodeRenamedEventHandler NodeRenamed;

		// Token: 0x14000034 RID: 52
		// (add) Token: 0x060004B8 RID: 1208
		// (remove) Token: 0x060004B9 RID: 1209
		event SVNodeMovedEventHandler NodeMoved;

		// Token: 0x14000035 RID: 53
		// (add) Token: 0x060004BA RID: 1210
		// (remove) Token: 0x060004BB RID: 1211
		event SVNodePropertyModifiedEventHandler NodePropertyModified;
	}
}
