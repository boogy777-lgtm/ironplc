using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IParser : IParser5, IParser4, IParser3, IParser2, IParser
	{
		IScanner UsedScanner { get; }

		Guid MessageGuid { get; }

		_IStatement ParsePragma(out bool bError, IMinimalPosition errorpos, string stPragma);

		_IStatement ParseInterfaceSnippet();

		_ISignature _ParseInterface(_IPreCompileContext precom, string stCompilerDefines);

		_IExpression ParseSTOperand(out bool bError);

		_IType ParseType();

		string LoadString(MessageId mid, params object[] args);

		void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args);

		void AddMessageST(_IExprement exp, Severity severity, MessageId nErrorId, params object[] args);

		void AddErrorST(_IExprement exp, IToken tokenPos, MessageId nErrorId, params object[] args);

		void AddMessageST(_IExprement exp, IToken tokenPos, Severity severity, MessageId nErrorId, params object[] args);

		_IStatement ParseST(bool bLibrary);

		_IStatement ParseST();
	}
}
