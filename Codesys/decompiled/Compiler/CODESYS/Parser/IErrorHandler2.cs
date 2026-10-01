using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IErrorHandler2 : IErrorHandler
	{
		IEnumerable<IMessage> GetMessages(IEnumerable<IPOUSyntax> topLevelPOUs);
	}
}
