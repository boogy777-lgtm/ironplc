using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200010F RID: 271
	// (Invoke) Token: 0x0600042C RID: 1068
	[ReleasedDelegate]
	public delegate void ObjectWasAddedHandler<TVirtualPSNode>(TVirtualPSNode virtualPSNode, Guid parentGuid, Guid newGuid);
}
