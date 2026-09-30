using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000111 RID: 273
	public enum ObjectAdditionProgressType
	{
		// Token: 0x04000149 RID: 329
		[ReleasedEnumMember]
		None,
		// Token: 0x0400014A RID: 330
		[ReleasedEnumMember]
		Loaded,
		// Token: 0x0400014B RID: 331
		[ReleasedEnumMember]
		Added,
		// Token: 0x0400014C RID: 332
		[ReleasedEnumMember]
		Merged,
		// Token: 0x0400014D RID: 333
		[ReleasedEnumMember]
		ChildrenAdded,
		// Token: 0x0400014E RID: 334
		[ReleasedEnumMember]
		AdditionEventsCalled
	}
}
