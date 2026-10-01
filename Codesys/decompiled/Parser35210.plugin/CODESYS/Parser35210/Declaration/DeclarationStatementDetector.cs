using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35210.Declaration
{
	internal class DeclarationStatementDetector : IStatementVisitor<bool>
	{
		public static bool CheckIfContainsDeclarationSyntax(_IPragmaIfStatement statement)
		{
			DeclarationStatementDetector visitor = new DeclarationStatementDetector();
			return statement.AcceptStatementVisitor(visitor);
		}

		public bool visit(_ISequenceStatement statement)
		{
			foreach (_IStatement statement2 in statement._StatementList)
			{
				if (statement2.AcceptStatementVisitor(this))
				{
					return true;
				}
			}
			return false;
		}

		public bool visit(_ICommentStatement statement)
		{
			return false;
		}

		public bool visit(_IPragmaStatement statement)
		{
			return false;
		}

		public bool visit(_IPragmaIfStatement statement)
		{
			if (statement.IfThen.AcceptStatementVisitor(this))
			{
				return true;
			}
			if (statement.IfElse.AcceptStatementVisitor(this))
			{
				return true;
			}
			if (statement.ElseIfs != null)
			{
				IElseIf[] elseIfs = statement.ElseIfs;
				for (int i = 0; i < elseIfs.Length; i++)
				{
					if (elseIfs[i].Controlled is _ISequenceStatement statement2 && statement2.AcceptStatementVisitor(this))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool visit(_IVariableDeclarationStatement statement)
		{
			return true;
		}

		public bool visit(_IVariableDeclarationListStatement statement)
		{
			return true;
		}
	}
}
