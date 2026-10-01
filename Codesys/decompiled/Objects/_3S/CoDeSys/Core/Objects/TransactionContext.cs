using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200008F RID: 143
	public enum TransactionContext
	{
		// Token: 0x040000D0 RID: 208
		[ReleasedEnumMember]
		Normal,
		// Token: 0x040000D1 RID: 209
		[ReleasedEnumMember]
		DuringUndo,
		// Token: 0x040000D2 RID: 210
		[ReleasedEnumMember]
		DuringRedo
	}
}
