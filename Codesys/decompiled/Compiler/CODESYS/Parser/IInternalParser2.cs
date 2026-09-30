using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IInternalParser2 : IInternalParser
	{
		void CreateLanguageModelOfRawST(IPOUSyntax[] pouSyntax, Guid sourceObjectGuid, Guid parentGuid, ILanguageModel dstLanguageModel);

		IPOUSyntax[] ParsePOUs(out IEnumerable<IMessage> parserErrors);
	}
}
