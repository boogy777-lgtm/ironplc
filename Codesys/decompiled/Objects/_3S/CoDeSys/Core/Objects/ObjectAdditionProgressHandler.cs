using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000110 RID: 272
	// (Invoke) Token: 0x06000430 RID: 1072
	[ReleasedDelegate]
	public delegate void ObjectAdditionProgressHandler<TVirtualPSNode>(TVirtualPSNode virtualPSNode, Guid parentGuid, Guid newGuid, ObjectAdditionProgressType type);
}
