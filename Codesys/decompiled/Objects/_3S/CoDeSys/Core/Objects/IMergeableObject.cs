using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200014B RID: 331
	[ReleasedInterface]
	public interface IMergeableObject
	{
		// Token: 0x060004F7 RID: 1271
		IObjectMerger CreateMerger(IObject obj);
	}
}
