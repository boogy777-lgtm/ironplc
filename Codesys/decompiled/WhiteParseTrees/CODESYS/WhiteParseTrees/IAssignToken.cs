using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IAssignToken : ICallAssignToken, IAnyAssignmentToken, IWhiteOperatorToken, IWhiteToken, INode
	{
	}
}
