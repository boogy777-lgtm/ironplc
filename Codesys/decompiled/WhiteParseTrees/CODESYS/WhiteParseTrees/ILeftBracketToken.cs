using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ILeftBracketToken : IAnyBraceLeftToken, IWhiteOperatorToken, IWhiteToken, INode, IAccessPathToken
	{
	}
}
