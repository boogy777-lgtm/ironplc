using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x02000007 RID: 7
	// (Invoke) Token: 0x0600000F RID: 15
	[ReleasedDelegate]
	public delegate void MessageDetailsHandler(IMessage message, object data);
}
