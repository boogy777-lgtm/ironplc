using System;
using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x02000011 RID: 17
	[ReleasedClass]
	[Serializable]
	public class MessagesRemovedEventArgs : EventArgs
	{
		// Token: 0x06000031 RID: 49 RVA: 0x00002067 File Offset: 0x00000267
		public MessagesRemovedEventArgs(IMessageCategory category, ICollection messages, Predicate<IMessage> match)
		{
			this._category = category;
			this._messages = messages;
			this._match = match;
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000032 RID: 50 RVA: 0x00002084 File Offset: 0x00000284
		public IMessageCategory Category
		{
			get
			{
				return this._category;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000033 RID: 51 RVA: 0x0000208C File Offset: 0x0000028C
		public ICollection Messages
		{
			get
			{
				return this._messages;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000034 RID: 52 RVA: 0x00002094 File Offset: 0x00000294
		public Predicate<IMessage> Match
		{
			get
			{
				return this._match;
			}
		}

		// Token: 0x0400000A RID: 10
		private IMessageCategory _category;

		// Token: 0x0400000B RID: 11
		private ICollection _messages;

		// Token: 0x0400000C RID: 12
		private Predicate<IMessage> _match;
	}
}
