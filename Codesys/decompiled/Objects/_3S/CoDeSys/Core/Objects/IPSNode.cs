using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000117 RID: 279
	[ReleasedInterface]
	public interface IPSNode : IComparable<IPSNode>, IComparable
	{
		// Token: 0x17000190 RID: 400
		// (get) Token: 0x0600043B RID: 1083
		int ProjectHandle { get; }

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x0600043C RID: 1084
		Guid ObjectGuid { get; }

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x0600043D RID: 1085
		Type ObjectType { get; }

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x0600043E RID: 1086
		Type[] EmbeddedObjectTypes { get; }

		// Token: 0x0600043F RID: 1087
		string GetName(bool bResolveLocalizedDisplayName);

		// Token: 0x06000440 RID: 1088
		string[] GetNamePath();

		// Token: 0x06000441 RID: 1089
		string GetFullName();

		// Token: 0x06000442 RID: 1090
		string GetDottedFullName();

		// Token: 0x06000443 RID: 1091
		void Rename(string stNewName);

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000444 RID: 1092
		bool OrderedSubObjects { get; }

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000445 RID: 1093
		int Index { get; }

		// Token: 0x06000446 RID: 1094
		int GetLevel();

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x06000447 RID: 1095
		bool IsFolder { get; }

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06000448 RID: 1096
		IPSNode ParentNode { get; }

		// Token: 0x06000449 RID: 1097
		IPSNode[] GetNodes();

		// Token: 0x0600044A RID: 1098
		IPSNode FindNode(Guid objectGuid, bool bRecursive);

		// Token: 0x0600044B RID: 1099
		IPSNode AddNode(Guid objectGuid, IObject obj, string stName, int nIndex);

		// Token: 0x0600044C RID: 1100
		IPSNode AddNodeUnchecked(Guid objectGuid, IObject obj, string stName, int nIndex);

		// Token: 0x0600044D RID: 1101
		IObject GetObject(bool bToModify);

		// Token: 0x0600044E RID: 1102
		void Export(out byte[] metaObjectData, out byte[] objectData);

		// Token: 0x0600044F RID: 1103
		void Export(Stream metaObjectStream, Stream objectStream, Guid archiveWriterGuid);

		// Token: 0x06000450 RID: 1104
		void Remove();

		// Token: 0x06000451 RID: 1105
		void RemoveWithoutParentCheck();

		// Token: 0x06000452 RID: 1106
		string GetUniqueName(Guid namespaceGuid, string stBaseName, out Guid conflictingObjectGuid);

		// Token: 0x06000453 RID: 1107
		void Move(IPSNode newParentNode, int nNewIndex);

		// Token: 0x06000454 RID: 1108
		bool CanPasteFromStream(Stream stream);

		// Token: 0x06000455 RID: 1109
		bool CanPasteFromStream(Stream stream, Guid archiveReaderGuid);

		// Token: 0x06000456 RID: 1110
		void PasteFromStream(Stream stream, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler);

		// Token: 0x06000457 RID: 1111
		void PasteFromStream(Stream stream, Guid archiveReaderGuid, EventHandler<PSPasteEventArgs> progressHandler, EventHandler<PSPasteConflictEventArgs> conflictHandler, EventHandler<PSPasteSkipEventArgs> skipHandler);
	}
}
