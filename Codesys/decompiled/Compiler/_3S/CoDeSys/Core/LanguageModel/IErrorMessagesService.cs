using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IErrorMessagesService
	{
		IMessage[] GetExprementMessages(IExprement expr);
	}
}
