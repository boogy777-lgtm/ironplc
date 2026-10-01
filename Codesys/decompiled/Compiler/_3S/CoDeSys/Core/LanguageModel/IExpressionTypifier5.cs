using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpressionTypifier5 : IExpressionTypifier4, IExpressionTypifier3, IExpressionTypifier2, IExpressionTypifier
	{
		void TypifyAndCheckStatement(IStatement statement, out IEnumerable<IMessage> messages);
	}
}
