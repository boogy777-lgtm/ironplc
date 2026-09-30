using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompiledPOUWithParseTreeProvider
	{
		IParseTreeProvider ParseTreeProvider { get; set; }

		_IStatement ParseTreeRaw { get; set; }

		_IStatement CreateTemporaryRedTree();
	}
}
