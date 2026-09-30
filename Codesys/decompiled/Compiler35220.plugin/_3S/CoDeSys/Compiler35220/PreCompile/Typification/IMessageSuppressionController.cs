using System;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification
{
	// Token: 0x020001B9 RID: 441
	public interface IMessageSuppressionController
	{
		// Token: 0x0600204D RID: 8269
		MessageHandling HandleMessage(MessageId mid, Severity severity);
	}
}
