using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpressionTypifier3 : IExpressionTypifier2, IExpressionTypifier
	{
		void TypifyStatement(IStatement statement, out IEnumerable<IMessage> messages);

		void TypifyExpression(IExpression expression, out IEnumerable<IMessage> messages);
	}
}
