using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35210.Declaration
{
	internal static class StatementExtensions
	{
		internal static T AcceptStatementVisitor<T>(this _IStatement statement, IStatementVisitor<T> visitor)
		{
			if (!(statement is _ICommentStatement statement2))
			{
				if (!(statement is _ISequenceStatement statement3))
				{
					if (!(statement is _IPragmaStatement statement4))
					{
						if (!(statement is _IPragmaIfStatement statement5))
						{
							if (!(statement is _IVariableDeclarationStatement statement6))
							{
								if (statement is _IVariableDeclarationListStatement statement7)
								{
									return visitor.visit(statement7);
								}
								return default(T);
							}
							return visitor.visit(statement6);
						}
						return visitor.visit(statement5);
					}
					return visitor.visit(statement4);
				}
				return visitor.visit(statement3);
			}
			return visitor.visit(statement2);
		}
	}
}
