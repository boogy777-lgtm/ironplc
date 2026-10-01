using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IParseTreeProvider
	{
		ICompactedParseTreeInformation CompactedParseTreeInformation { get; set; }

		_IStatement GetParseTree();

		_IStatement CreateTemporaryRedTree();

		void CreateParseTreeForCompiledPOU(_ICompiledPOU pouRet);

		void SetParseTreeWithSideEffects(_IStatement parseTree);

		void SetParseTreeDirectly(_IStatement parseTree);

		void DuplicateParseTreeForCompilation();

		_IStatement GetParseTreeForSerialization(bool bDeleteComments);
	}
}
