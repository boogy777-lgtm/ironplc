using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35210.Declaration
{
	internal class StatementElement : SyntaxElement
	{
		internal _IStatement Statement { get; }

		internal StatementElement(_IStatement statement)
		{
			Statement = statement;
		}
	}
}
