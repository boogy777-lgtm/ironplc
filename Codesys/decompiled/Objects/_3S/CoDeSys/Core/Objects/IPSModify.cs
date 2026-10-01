using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000115 RID: 277
	[ReleasedInterface]
	public interface IPSModify : IPSChange
	{
		// Token: 0x1700018B RID: 395
		// (get) Token: 0x06000436 RID: 1078
		object Editor { get; }
	}
}
