using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.ObjectManager
{
	// Token: 0x02000007 RID: 7
	[ReleasedInterface]
	public interface IStructuredViewDeterminator
	{
		// Token: 0x06000026 RID: 38
		IStructuredView GetStructuredViewForRootObjectType(int projectHandle, Type rootObjectType);

		// Token: 0x06000027 RID: 39
		Guid GetStructuredViewGuidForRootObjectType(Type rootObjectType);
	}
}
