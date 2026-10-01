using System;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Compiler35220
{
	// Token: 0x0200000B RID: 11
	public static class IAPEnvironmentFacadeEx
	{
		// Token: 0x0600008B RID: 139 RVA: 0x0000281C File Offset: 0x00000A1C
		public static void AddMessage(this IAPEnvironmentFacade self, IMessageCategory category, IMessage message)
		{
			self.MessageStorage.AddMessage(category, message);
		}

		// Token: 0x0600008C RID: 140 RVA: 0x0000282C File Offset: 0x00000A2C
		public static IMessage[] GetMessages(this IAPEnvironmentFacade self, IMessageCategory category, Severity severity)
		{
			return self.MessageStorage.GetMessages(category, severity);
		}

		// Token: 0x0600008D RID: 141 RVA: 0x0000283C File Offset: 0x00000A3C
		public static void ClearMessages(this IAPEnvironmentFacade self, IMessageCategory category)
		{
			self.MessageStorage.ClearMessages(category);
		}

		// Token: 0x0600008E RID: 142 RVA: 0x0000284C File Offset: 0x00000A4C
		public static Version CompilerVersionToUseInternal(this IAPEnvironmentFacade self)
		{
			return self.CompilerVersionSettings.CompilerVersionToUseInternal();
		}
	}
}
