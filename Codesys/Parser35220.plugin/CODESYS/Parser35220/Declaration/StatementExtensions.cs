using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000054 RID: 84
	internal static class StatementExtensions
	{
		// Token: 0x06000576 RID: 1398 RVA: 0x0001702C File Offset: 0x0001522C
		internal static T AcceptStatementVisitor<T>(this _IStatement statement, IStatementVisitor<T> visitor)
		{
			_ICommentStatement icommentStatement = statement as _ICommentStatement;
			if (icommentStatement != null)
			{
				return visitor.visit(icommentStatement);
			}
			_ISequenceStatement isequenceStatement = statement as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				return visitor.visit(isequenceStatement);
			}
			_IPragmaStatement ipragmaStatement = statement as _IPragmaStatement;
			if (ipragmaStatement != null)
			{
				return visitor.visit(ipragmaStatement);
			}
			_IPragmaIfStatement ipragmaIfStatement = statement as _IPragmaIfStatement;
			if (ipragmaIfStatement != null)
			{
				return visitor.visit(ipragmaIfStatement);
			}
			_IVariableDeclarationStatement ivariableDeclarationStatement = statement as _IVariableDeclarationStatement;
			if (ivariableDeclarationStatement != null)
			{
				return visitor.visit(ivariableDeclarationStatement);
			}
			_IVariableDeclarationListStatement ivariableDeclarationListStatement = statement as _IVariableDeclarationListStatement;
			if (ivariableDeclarationListStatement == null)
			{
				return default(T);
			}
			return visitor.visit(ivariableDeclarationListStatement);
		}
	}
}
