using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x02000004 RID: 4
	[ReleasedInterface]
	public interface IMessage3 : IMessage2, IMessage
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000009 RID: 9
		MessageDetailsHandler DetailsHandler { get; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600000A RID: 10
		object DetailsHandlerData { get; }
	}
}
