using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x02000006 RID: 6
	[Flags]
	[TypeGuid("{2f9d7c97-2099-4966-be47-5be6f702f0c4}")]
	public enum Severity : uint
	{
		// Token: 0x04000002 RID: 2
		[ReleasedEnumMember]
		FatalError = 1U,
		// Token: 0x04000003 RID: 3
		[ReleasedEnumMember]
		Error = 2U,
		// Token: 0x04000004 RID: 4
		[ReleasedEnumMember]
		Warning = 4U,
		// Token: 0x04000005 RID: 5
		[ReleasedEnumMember]
		Information = 8U,
		// Token: 0x04000006 RID: 6
		[ReleasedEnumMember]
		Text = 16U,
		// Token: 0x04000007 RID: 7
		[ReleasedEnumMember]
		SuppressedWarning = 32U,
		// Token: 0x04000008 RID: 8
		[ReleasedEnumMember]
		SuppressedInformation = 64U
	}
}
