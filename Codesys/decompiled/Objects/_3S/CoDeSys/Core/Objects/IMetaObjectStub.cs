using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000E9 RID: 233
	[ReleasedInterface]
	public interface IMetaObjectStub
	{
		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000394 RID: 916
		Guid ObjectGuid { get; }

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x06000395 RID: 917
		int ProjectHandle { get; }

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000396 RID: 918
		Type ObjectType { get; }

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000397 RID: 919
		Type[] EmbeddedObjectTypes { get; }

		// Token: 0x17000165 RID: 357
		// (get) Token: 0x06000398 RID: 920
		string Name { get; }

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x06000399 RID: 921
		Guid Namespace { get; }

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x0600039A RID: 922
		Guid[] SubObjectGuids { get; }

		// Token: 0x17000168 RID: 360
		// (get) Token: 0x0600039B RID: 923
		bool OrderedSubObjects { get; }

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x0600039C RID: 924
		int Index { get; }

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x0600039D RID: 925
		Guid ParentObjectGuid { get; }

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x0600039E RID: 926
		IObjectProperty[] Properties { get; }

		// Token: 0x0600039F RID: 927
		IObjectProperty GetProperty(Guid propertyGuid);
	}
}
