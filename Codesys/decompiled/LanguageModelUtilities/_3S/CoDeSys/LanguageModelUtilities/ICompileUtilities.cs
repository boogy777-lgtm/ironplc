using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICompileUtilities
	{
		IAddressInfoCreator CreateAddressInfoCreator();

		IExpression ParseAndTypifyExpression(Guid guidApplication, string stExpression, int iSignatureId, out IEnumerable<IMessage> mess);
	}
}
