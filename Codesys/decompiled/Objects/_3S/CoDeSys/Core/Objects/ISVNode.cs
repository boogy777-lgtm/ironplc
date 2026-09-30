using System;
using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000157 RID: 343
	[ReleasedInterface]
	public interface ISVNode : IComparable
	{
		// Token: 0x170001CE RID: 462
		// (get) Token: 0x06000516 RID: 1302
		bool OrderedSubObjects { get; }

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000517 RID: 1303
		int Index { get; }

		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06000518 RID: 1304
		ISVNode[] Children { get; }

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000519 RID: 1305
		int ChildCount { get; }

		// Token: 0x0600051A RID: 1306
		ISVNode GetChild(int nIndex);

		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x0600051B RID: 1307
		IStructuredView StructuredView { get; }

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x0600051C RID: 1308
		int ProjectHandle { get; }

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x0600051D RID: 1309
		Guid ObjectGuid { get; }

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x0600051E RID: 1310
		string Name { get; }

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x0600051F RID: 1311
		Type ObjectType { get; }

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000520 RID: 1312
		ISVNode Parent { get; }

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000521 RID: 1313
		bool IsFolder { get; }

		// Token: 0x06000522 RID: 1314
		void AddObject(Guid objectGuid, IObject obj, string stName, int nIndex);

		// Token: 0x06000523 RID: 1315
		void ExportObject(out byte[] metaObjectData, out byte[] objectData);

		// Token: 0x06000524 RID: 1316
		void Remove();

		// Token: 0x06000525 RID: 1317
		void Rename(string stNewName);

		// Token: 0x06000526 RID: 1318
		IMetaObjectStub GetMetaObjectStub();

		// Token: 0x06000527 RID: 1319
		IMetaObject GetObjectToRead();

		// Token: 0x06000528 RID: 1320
		IMetaObject GetObjectToModify();

		// Token: 0x06000529 RID: 1321
		void MoveObject(ISVNode newParentNode, int nNewIndex);

		// Token: 0x0600052A RID: 1322
		void RenameObject(string stNewName);

		// Token: 0x0600052B RID: 1323
		string GetFullName();

		// Token: 0x0600052C RID: 1324
		void CopyToStream(Stream stream);

		// Token: 0x0600052D RID: 1325
		bool CanPasteFromStream(Stream stream);

		// Token: 0x0600052E RID: 1326
		void PasteFromStream(Stream stream);
	}
}
