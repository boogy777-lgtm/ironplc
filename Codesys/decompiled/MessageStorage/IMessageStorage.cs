using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x0200000A RID: 10
	[ReleasedInterface]
	public interface IMessageStorage
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000015 RID: 21
		IMessageCategory[] Categories { get; }

		// Token: 0x06000016 RID: 22
		IMessage[] GetMessages(IMessageCategory category);

		// Token: 0x06000017 RID: 23
		IMessage[] GetMessages(IMessageCategory category, Severity severity);

		// Token: 0x06000018 RID: 24
		void AddMessage(IMessageCategory category, IMessage message);

		// Token: 0x06000019 RID: 25
		void ClearMessages(IMessageCategory category);

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600001A RID: 26
		// (remove) Token: 0x0600001B RID: 27
		event MessageCategoryEventHandler CategoryAdded;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x0600001C RID: 28
		// (remove) Token: 0x0600001D RID: 29
		event MessageCategoryEventHandler MessagesCleared;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x0600001E RID: 30
		// (remove) Token: 0x0600001F RID: 31
		event MessageEventHandler MessageAdded;
	}
}
