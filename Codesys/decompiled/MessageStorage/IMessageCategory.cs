using System;
using System.Drawing;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x02000008 RID: 8
	[ReleasedInterface]
	public interface IMessageCategory
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000012 RID: 18
		string Text { get; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000013 RID: 19
		Icon Icon { get; }
	}
}
