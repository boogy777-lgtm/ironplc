using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x0200000C RID: 12
	[ReleasedInterface]
	public interface IMessageStorage3 : IMessageStorage2, IMessageStorage
	{
		// Token: 0x06000023 RID: 35
		void RemoveCategory(IMessageCategory category);

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000024 RID: 36
		// (remove) Token: 0x06000025 RID: 37
		event EventHandler<MessageCategoryEventArgs> CategoryRemoved;
	}
}
