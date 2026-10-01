using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000103 RID: 259
	[Flags]
	public enum ProjectSourceFlags
	{
		// Token: 0x04000141 RID: 321
		[ReleasedEnumMember]
		None = 0,
		// Token: 0x04000142 RID: 322
		[ReleasedEnumMember]
		Temporary = 1,
		// Token: 0x04000143 RID: 323
		[ReleasedEnumMember]
		Persistent = 2,
		// Token: 0x04000144 RID: 324
		[ReleasedEnumMember]
		SimulateLoadEvents = 4
	}
}
