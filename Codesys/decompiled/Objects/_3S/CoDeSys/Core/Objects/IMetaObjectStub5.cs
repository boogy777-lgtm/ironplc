using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000ED RID: 237
	[ReleasedInterface]
	public interface IMetaObjectStub5 : IMetaObjectStub4, IMetaObjectStub3, IMetaObjectStub2, IMetaObjectStub
	{
		// Token: 0x1700016F RID: 367
		// (get) Token: 0x060003A3 RID: 931
		IIncompleteDeserializationInfo IncompleteDeserializationInfo { get; }
	}
}
