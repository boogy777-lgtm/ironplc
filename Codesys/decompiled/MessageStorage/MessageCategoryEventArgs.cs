using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Messages
{
	// Token: 0x0200000F RID: 15
	[ReleasedClass]
	public class MessageCategoryEventArgs : EventArgs
	{
		// Token: 0x0600002B RID: 43 RVA: 0x00002050 File Offset: 0x00000250
		public MessageCategoryEventArgs(IMessageCategory category)
		{
			this._category = category;
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600002C RID: 44 RVA: 0x0000205F File Offset: 0x0000025F
		public IMessageCategory Category
		{
			get
			{
				return this._category;
			}
		}

		// Token: 0x04000009 RID: 9
		private IMessageCategory _category;
	}
}
