using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000DC RID: 220
	[ReleasedInterface]
	public interface IGenericObjectServiceFactory
	{
		// Token: 0x06000371 RID: 881
		IGenericObjectService Create(Type type);
	}
}
