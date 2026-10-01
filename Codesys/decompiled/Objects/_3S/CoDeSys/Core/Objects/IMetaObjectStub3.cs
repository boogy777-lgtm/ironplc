using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000EB RID: 235
	[ReleasedInterface]
	public interface IMetaObjectStub3 : IMetaObjectStub2, IMetaObjectStub
	{
		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060003A1 RID: 929
		int ModificationCounter { get; }
	}
}
