using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000023 RID: 35
	[Flags]
	public enum ProjectWriteProtectionMode
	{
		// Token: 0x0400001C RID: 28
		[ReleasedEnumMember]
		None = 0,
		// Token: 0x0400001D RID: 29
		[ReleasedEnumMember]
		FileIsReadOnly = 1,
		// Token: 0x0400001E RID: 30
		[ReleasedEnumMember]
		ProjectHasBeenOpenedReadOnly = 2,
		// Token: 0x0400001F RID: 31
		[ReleasedEnumMember]
		ProjectIsMarkedAsReleased = 4,
		// Token: 0x04000020 RID: 32
		[ReleasedEnumMember]
		ProjectIsIncomplete = 8
	}
}
