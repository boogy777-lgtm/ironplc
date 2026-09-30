using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IOrNToken : IAnyOrToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
	}
}
