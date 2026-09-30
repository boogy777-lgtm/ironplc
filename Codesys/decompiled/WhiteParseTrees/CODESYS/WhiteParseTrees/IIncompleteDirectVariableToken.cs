using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IIncompleteDirectVariableToken : IWhiteToken, INode
	{
		DirectVariableLocation Location { get; }
	}
}
