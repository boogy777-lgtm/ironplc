using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IErrorHandler
	{
		void AddError(IToken tokenPos, MessageId ErrorId, params object[] args);

		void AddErrorSTWithToken(_IExprement exp, IToken tokenPos, MessageId nErrorId, params object[] args);

		void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args);

		void AddWarningST(_IExprement exp, MessageId nWrnId, params object[] args);

		string LoadString(MessageId mid, params object[] args);
	}
}
