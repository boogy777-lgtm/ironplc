using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000E6 RID: 230
	[ReleasedInterface]
	public interface IMetaObject7 : IMetaObject6, IMetaObject5, IMetaObject4, IMetaObject3, IMetaObject2, IMetaObject, IGenericObject, IArchivable, ICloneable, IComparable
	{
		// Token: 0x1700015D RID: 349
		// (get) Token: 0x0600038E RID: 910
		Guid ObjectTypeGuid { get; }

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x0600038F RID: 911
		IEnumerable<Guid> EmbeddedObjectTypeGuids { get; }

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000390 RID: 912
		IEnumerable<Guid> PropertyTypeGuids { get; }
	}
}
