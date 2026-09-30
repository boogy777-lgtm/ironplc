using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder2 : ILanguageModelBuilder
	{
		IMessage4 CreateCompilerMessage(IExprementPosition position, IExprement exp, string stMessage, Severity severity, ShowAttribute showatt, uint uiMessageId, string stMessagePrefix);

		IExpression ParseInitialisation(string stExpression);
	}
}
