using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileService3 : ILMPreCompileService2, ILMPreCompileService
	{
		IEnumerable<IMessage> GetExprementMessages(IExprement exprement);
	}
}
