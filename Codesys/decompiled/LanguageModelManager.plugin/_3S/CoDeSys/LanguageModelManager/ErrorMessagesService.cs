using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200003B RID: 59
	[TypeGuid("{CCCAF5C4-476C-41CE-B0DE-12C52307009F}")]
	public class ErrorMessagesService : IErrorMessagesService
	{
		// Token: 0x060002C5 RID: 709 RVA: 0x0000AC1E File Offset: 0x00009C1E
		public IMessage[] GetExprementMessages(IExprement expr)
		{
			if (expr == null)
			{
				throw new ArgumentNullException("expr");
			}
			return CompilerProxy.GetExprementMessages((Exprement)expr);
		}
	}
}
