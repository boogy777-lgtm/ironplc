using System;
using System.Drawing;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x02000005 RID: 5
	[ReleasedInterface]
	public interface IMessage4 : IMessage3, IMessage2, IMessage
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600000B RID: 11
		Icon Icon { get; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600000C RID: 12
		uint? Number { get; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600000D RID: 13
		string Prefix { get; }
	}
}
