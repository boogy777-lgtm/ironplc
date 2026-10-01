using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000EE RID: 238
	[ReleasedInterface]
	public interface IMetaObjectStub6 : IMetaObjectStub5, IMetaObjectStub4, IMetaObjectStub3, IMetaObjectStub2, IMetaObjectStub
	{
		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060003A4 RID: 932
		Guid ObjectTypeGuid { get; }

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060003A5 RID: 933
		IEnumerable<Guid> EmbeddedObjectTypeGuids { get; }

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060003A6 RID: 934
		IEnumerable<Guid> PropertyTypeGuids { get; }
	}
}
