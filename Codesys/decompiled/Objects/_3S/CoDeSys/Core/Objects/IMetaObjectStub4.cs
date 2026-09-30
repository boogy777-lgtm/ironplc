using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000EC RID: 236
	[ReleasedInterface]
	public interface IMetaObjectStub4 : IMetaObjectStub3, IMetaObjectStub2, IMetaObjectStub
	{
		// Token: 0x1700016E RID: 366
		// (get) Token: 0x060003A2 RID: 930
		bool DeserializedIncompletely { get; }
	}
}
