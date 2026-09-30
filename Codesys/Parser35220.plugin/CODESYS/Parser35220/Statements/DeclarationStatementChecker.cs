using System;
using System.Linq;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x0200002C RID: 44
	public static class DeclarationStatementChecker
	{
		// Token: 0x0600030F RID: 783 RVA: 0x0000E5D8 File Offset: 0x0000C7D8
		private static bool CheckForExpectedStatement(_IStatement state, ref bool bBeforeDeclaration)
		{
			bool flag = state is _IPOUDeclarationStatement || state is _ITypeDeclarationStatement;
			_IVariableDeclarationListStatement ivariableDeclarationListStatement = state as _IVariableDeclarationListStatement;
			bool flag2 = ivariableDeclarationListStatement != null && (ivariableDeclarationListStatement.GetFlag(8192L) || ivariableDeclarationListStatement.GetFlag(4096L));
			bool flag3 = state is _ICommentStatement || state is _IPragmaStatement || state is _IPragmaIfStatement || state is _IErrorStatement || state is _IEmptyStatement || flag2;
			bool result = bBeforeDeclaration ? (flag3 || flag) : flag3;
			if (flag)
			{
				bBeforeDeclaration = false;
			}
			return result;
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0000E664 File Offset: 0x0000C864
		private static bool HasSyntaxErrors(_IStatement statement)
		{
			IMessage[] allMessages = statement.GetAllMessages();
			if (allMessages == null)
			{
				return false;
			}
			return allMessages.Any((IMessage x) => x.Severity == 2 || x.Severity == 1);
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0000E6A4 File Offset: 0x0000C8A4
		internal static void PreventAnyCodeAfterTypeDeclaration(_IStatement state, ParserContext context)
		{
			if (DeclarationStatementChecker.HasSyntaxErrors(state))
			{
				return;
			}
			if (!DeclarationStatementChecker.ContainsDeclarationStatement(state))
			{
				return;
			}
			_ISequenceStatement isequenceStatement = (_ISequenceStatement)state;
			bool flag = true;
			for (int i = 0; i < isequenceStatement._StatementList.Count<_IStatement>(); i++)
			{
				_IStatement istatement = isequenceStatement._StatementList[i];
				if (!DeclarationStatementChecker.CheckForExpectedStatement(istatement, ref flag))
				{
					context.ErrorHandler.AddErrorST(istatement, 578, Array.Empty<object>());
				}
			}
		}

		// Token: 0x06000312 RID: 786 RVA: 0x0000E710 File Offset: 0x0000C910
		private static bool ContainsDeclarationStatement(_IStatement state)
		{
			_ISequenceStatement isequenceStatement = state as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				return isequenceStatement._StatementList.Any((_IStatement x) => x is _IPOUDeclarationStatement || x is _ITypeDeclarationStatement);
			}
			return false;
		}
	}
}
