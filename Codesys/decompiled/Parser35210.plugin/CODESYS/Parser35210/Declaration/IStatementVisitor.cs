using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35210.Declaration
{
	internal interface IStatementVisitor<out T>
	{
		T visit(_ISequenceStatement statement);

		T visit(_ICommentStatement statement);

		T visit(_IPragmaStatement statement);

		T visit(_IPragmaIfStatement statement);

		T visit(_IVariableDeclarationStatement statement);

		T visit(_IVariableDeclarationListStatement statement);
	}
}
