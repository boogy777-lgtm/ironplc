using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IInternalParser
	{
		bool ImplicitAnyway { get; set; }

		IScanner9 Scanner { get; }

		Guid MessageGuid { get; set; }

		_IExpression[] ParseSTSnippet();

		_IStatement ParseST(bool bLibrary);

		_IStatement ParseInterfaceStatement();

		_IExpression ParseAssignExp(out bool bError);

		_IExpression ParseSTOperand(out bool bError);

		_IExpression ParseInitialisation();

		_IType ParseType();

		TokenType Next(out IToken token, bool bWithPragma, bool bWithComment);

		Operator MatchOperator(params Operator[] ops);

		_IStatement ParsePragma(out bool bError, IMinimalPosition errorpos, string stPragma);

		void ExtendForLoop(_IForStatement forstatement);

		_ISequenceStatement ParseRawST();

		IPOUSyntax[] ParsePOUs();
	}
}
