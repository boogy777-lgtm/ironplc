using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpressionTypifier6 : IExpressionTypifier5, IExpressionTypifier4, IExpressionTypifier3, IExpressionTypifier2, IExpressionTypifier
	{
		void TypifyAndCheckExpression(IExpression expression, out IEnumerable<IMessage> messages);
	}
}
