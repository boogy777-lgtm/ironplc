using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200014C RID: 332
	[ReleasedInterface]
	public interface IObject : IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x060004F8 RID: 1272
		// (set) Token: 0x060004F9 RID: 1273
		IMetaObject MetaObject { get; set; }

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x060004FA RID: 1274
		Guid Namespace { get; }

		// Token: 0x060004FB RID: 1275
		bool CheckName(string stName);

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x060004FC RID: 1276
		bool CanRename { get; }

		// Token: 0x060004FD RID: 1277
		void HandleRenamed();

		// Token: 0x060004FE RID: 1278
		string GetPositionText(long nPosition);

		// Token: 0x060004FF RID: 1279
		string GetContentString(ref long nPosition, ref int nLength, bool bWord);

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000500 RID: 1280
		IEmbeddedObject[] EmbeddedObjects { get; }

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000501 RID: 1281
		IUniqueIdGenerator UniqueIdGenerator { get; }

		// Token: 0x06000502 RID: 1282
		bool AcceptsParentObject(IObject parentObject);

		// Token: 0x06000503 RID: 1283
		bool AcceptsChildObject(Type childObjectType);

		// Token: 0x06000504 RID: 1284
		int CheckRelationships(IObject parentObject, IObject[] childObjects);
	}
}
