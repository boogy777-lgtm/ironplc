using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	public static class DeclarationStatementChecker
	{
		private static bool CheckForExpectedStatement(_IStatement state, ref bool bBeforeDeclaration)
		{
			bool flag = state is _IPOUDeclarationStatement || state is _ITypeDeclarationStatement;
			bool flag2 = state is _IVariableDeclarationListStatement iVariableDeclarationListStatement && (iVariableDeclarationListStatement.GetFlag(VarFlag.Global) || iVariableDeclarationListStatement.GetFlag(VarFlag.VarConfig));
			bool flag3 = state is _ICommentStatement || state is _IPragmaStatement || state is _IPragmaIfStatement || state is _IErrorStatement || state is _IEmptyStatement || flag2;
			bool result = (bBeforeDeclaration ? (flag3 || flag) : flag3);
			if (flag)
			{
				bBeforeDeclaration = false;
			}
			return result;
		}

		private static bool HasSyntaxErrors(_IStatement statement)
		{
			return statement.GetAllMessages()?.Any((IMessage x) => x.Severity == Severity.Error || x.Severity == Severity.FatalError) ?? false;
		}

		internal static void PreventAnyCodeAfterTypeDeclaration(_IStatement state, ParserContext context)
		{
			if (HasSyntaxErrors(state) || !ContainsDeclarationStatement(state))
			{
				return;
			}
			_ISequenceStatement iSequenceStatement = (_ISequenceStatement)state;
			bool bBeforeDeclaration = true;
			for (int i = 0; i < iSequenceStatement._StatementList.Count(); i++)
			{
				_IStatement iStatement = iSequenceStatement._StatementList[i];
				if (!CheckForExpectedStatement(iStatement, ref bBeforeDeclaration))
				{
					context.ErrorHandler.AddErrorST(iStatement, MessageId.Err_UnexpectedStatement);
				}
			}
		}

		private static bool ContainsDeclarationStatement(_IStatement state)
		{
			if (state is _ISequenceStatement iSequenceStatement)
			{
				return iSequenceStatement._StatementList.Any((_IStatement x) => x is _IPOUDeclarationStatement || x is _ITypeDeclarationStatement);
			}
			return false;
		}
	}
}
