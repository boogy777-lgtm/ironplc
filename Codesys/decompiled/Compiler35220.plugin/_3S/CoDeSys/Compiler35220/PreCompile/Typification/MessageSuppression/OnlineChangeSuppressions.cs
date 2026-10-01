using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification.MessageSuppression
{
	// Token: 0x020001BA RID: 442
	public class OnlineChangeSuppressions : IMessageSuppressionController
	{
		// Token: 0x17000601 RID: 1537
		// (get) Token: 0x0600204E RID: 8270 RVA: 0x0006DD78 File Offset: 0x0006BF78
		public static IMessageSuppressionController Instance { get; } = new OnlineChangeSuppressions();

		// Token: 0x0600204F RID: 8271 RVA: 0x0006DD80 File Offset: 0x0006BF80
		private OnlineChangeSuppressions()
		{
		}

		// Token: 0x06002050 RID: 8272 RVA: 0x0006DD88 File Offset: 0x0006BF88
		public MessageHandling HandleMessage(MessageId mid, Severity severity)
		{
			if (mid == MessageId.Err_NoAssign)
			{
				return MessageHandling.Ignore;
			}
			return MessageHandling.Report;
		}

		// Token: 0x04000542 RID: 1346
		[CompilerGenerated]
		private static readonly IMessageSuppressionController \u0001;
	}
}
