using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification.MessageSuppression
{
	// Token: 0x020001BC RID: 444
	public class IgnoreWarnings : IMessageSuppressionController
	{
		// Token: 0x17000603 RID: 1539
		// (get) Token: 0x06002056 RID: 8278 RVA: 0x0006DDD0 File Offset: 0x0006BFD0
		public static IMessageSuppressionController Instance { get; } = new IgnoreWarnings();

		// Token: 0x06002057 RID: 8279 RVA: 0x0006DDD8 File Offset: 0x0006BFD8
		private IgnoreWarnings()
		{
		}

		// Token: 0x06002058 RID: 8280 RVA: 0x0006DDE0 File Offset: 0x0006BFE0
		public MessageHandling HandleMessage(MessageId mid, Severity severity)
		{
			if (severity == Severity.Warning)
			{
				return MessageHandling.Suppress;
			}
			return MessageHandling.Report;
		}

		// Token: 0x04000544 RID: 1348
		[CompilerGenerated]
		private static readonly IMessageSuppressionController \u0001;
	}
}
