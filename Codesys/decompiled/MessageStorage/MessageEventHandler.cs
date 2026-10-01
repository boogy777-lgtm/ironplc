using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x0200000D RID: 13
	// (Invoke) Token: 0x06000027 RID: 39
	[ReleasedDelegate]
	public delegate void MessageEventHandler(IMessageCategory category, IMessage message);
}
