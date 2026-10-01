using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification.MessageSuppression
{
	// Token: 0x020001BD RID: 445
	public class NoSuppressions : IMessageSuppressionController
	{
		// Token: 0x17000604 RID: 1540
		// (get) Token: 0x0600205A RID: 8282 RVA: 0x0006DDF8 File Offset: 0x0006BFF8
		public static IMessageSuppressionController Instance { get; } = new NoSuppressions();

		// Token: 0x0600205B RID: 8283 RVA: 0x0006DE00 File Offset: 0x0006C000
		private NoSuppressions()
		{
		}

		// Token: 0x0600205C RID: 8284 RVA: 0x0006DE08 File Offset: 0x0006C008
		public MessageHandling HandleMessage(MessageId mid, Severity severity)
		{
			return MessageHandling.Report;
		}

		// Token: 0x04000545 RID: 1349
		[CompilerGenerated]
		private static readonly IMessageSuppressionController \u0001;
	}
}
