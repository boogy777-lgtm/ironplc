using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.ObjectManager
{
	// Token: 0x02000008 RID: 8
	[ReleasedInterface]
	public interface IStructuredViewDeterminationProvider
	{
		// Token: 0x06000028 RID: 40
		Guid? GetStructuredViewForRootObjectType(Type rootObjectType);
	}
}
