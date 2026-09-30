using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompiledPOUWithCompactedParseTree
	{
		ICompactedParseTreeInformation CompactedParseTreeInformation { get; set; }

		_IStatement OriginalParseTree { get; }
	}
}
