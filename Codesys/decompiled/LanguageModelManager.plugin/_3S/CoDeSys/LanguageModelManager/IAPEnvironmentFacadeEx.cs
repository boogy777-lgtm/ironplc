using System;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000020 RID: 32
	public static class IAPEnvironmentFacadeEx
	{
		// Token: 0x06000125 RID: 293 RVA: 0x00003420 File Offset: 0x00002420
		public static void AddMessage(this IAPEnvironmentFacade self, IMessageCategory category, IMessage message)
		{
			self.MessageStorage.AddMessage(category, message);
		}

		// Token: 0x06000126 RID: 294 RVA: 0x0000342F File Offset: 0x0000242F
		public static IMessage[] GetMessages(this IAPEnvironmentFacade self, IMessageCategory category, Severity severity)
		{
			return self.MessageStorage.GetMessages(category, severity);
		}

		// Token: 0x06000127 RID: 295 RVA: 0x0000343E File Offset: 0x0000243E
		public static void ClearMessages(this IAPEnvironmentFacade self, IMessageCategory category)
		{
			self.MessageStorage.ClearMessages(category);
		}
	}
}
