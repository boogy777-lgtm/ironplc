using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileTypifier
	{
		bool CheckPOUCode(Guid guidObject, out IEnumerable<IMessage> messages);

		bool CheckSignature(Guid guidSignature, out IEnumerable<IMessage> messages);

		IStatement CreateTypifiedParseTree(Guid guidObject, bool bAddImplicitConversions);

		IStatement TypifyStatement(IStatement stmt, Guid guidObject, bool bAddImplicitConversions);
	}
}
