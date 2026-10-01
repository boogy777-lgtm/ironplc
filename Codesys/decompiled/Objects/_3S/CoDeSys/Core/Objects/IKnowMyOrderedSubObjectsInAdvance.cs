using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200014A RID: 330
	[ReleasedInterface]
	public interface IKnowMyOrderedSubObjectsInAdvance : IOrderedSubObjects
	{
		// Token: 0x060004F6 RID: 1270
		int GetEnvisionedIndexOf(int nProjectHandle, Guid objectGuid);
	}
}
