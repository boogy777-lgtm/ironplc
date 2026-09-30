using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICompileUtilities7 : ICompileUtilities6, ICompileUtilities5, ICompileUtilities4, ICompileUtilities3, ICompileUtilities2, ICompileUtilities
	{
		bool ContainsOnlineVarReferenceRelevantError(IEnumerable<IMessage> messages);

		IMessage GetOnlineVarReferenceRelevantError(IEnumerable<IMessage> messages);

		IExpression ParseAndTypifyExpression2(Guid guidApplication, string stExpression, int iSignatureId, out IEnumerable<IMessage> mess, bool bUsedForVarReference);
	}
}
