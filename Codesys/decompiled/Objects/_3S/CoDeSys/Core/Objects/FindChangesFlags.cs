using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200008E RID: 142
	[Flags]
	public enum FindChangesFlags
	{
		// Token: 0x040000CA RID: 202
		[ReleasedEnumMember]
		Node = 1,
		// Token: 0x040000CB RID: 203
		[ReleasedEnumMember]
		ImmediateParent = 2,
		// Token: 0x040000CC RID: 204
		[ReleasedEnumMember]
		AnyParent = 4,
		// Token: 0x040000CD RID: 205
		[ReleasedEnumMember]
		ImmediateChild = 8,
		// Token: 0x040000CE RID: 206
		[ReleasedEnumMember]
		AnyChild = 16
	}
}
