using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification.MessageSuppression
{
	// Token: 0x020001BB RID: 443
	public class IgnoreWarningsAndErrors : IMessageSuppressionController
	{
		// Token: 0x17000602 RID: 1538
		// (get) Token: 0x06002052 RID: 8274 RVA: 0x0006DDA4 File Offset: 0x0006BFA4
		public static IMessageSuppressionController Instance { get; } = new IgnoreWarningsAndErrors();

		// Token: 0x06002053 RID: 8275 RVA: 0x0006DDAC File Offset: 0x0006BFAC
		public MessageHandling HandleMessage(MessageId mid, Severity severity)
		{
			if (severity == Severity.Warning || severity == Severity.Error)
			{
				return MessageHandling.Ignore;
			}
			return MessageHandling.Report;
		}

		// Token: 0x04000543 RID: 1347
		[CompilerGenerated]
		private static readonly IMessageSuppressionController \u0001;
	}
}
