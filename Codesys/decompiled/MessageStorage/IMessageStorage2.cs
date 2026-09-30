using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x0200000B RID: 11
	[ReleasedInterface]
	public interface IMessageStorage2 : IMessageStorage
	{
		// Token: 0x06000020 RID: 32
		void RemoveMessages(IMessageCategory category, Predicate<IMessage> match);

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000021 RID: 33
		// (remove) Token: 0x06000022 RID: 34
		event EventHandler<MessagesRemovedEventArgs> MessagesRemoved;
	}
}
