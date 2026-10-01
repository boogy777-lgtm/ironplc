using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ILeftParenthesisToken : IAnyBraceLeftToken, IWhiteOperatorToken, IWhiteToken, INode, IAccessPathToken
	{
	}
}
