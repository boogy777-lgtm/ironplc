using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200014E RID: 334
	[ReleasedInterface]
	public interface IObjectMerger
	{
		// Token: 0x06000506 RID: 1286
		void Merge(IObject sourceObject, int nTargetProjectHandle, Guid targetObjectGuid);
	}
}
