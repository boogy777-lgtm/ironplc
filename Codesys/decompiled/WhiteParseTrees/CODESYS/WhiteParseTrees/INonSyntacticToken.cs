using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(2)]
	[ReleasedInterface]
	public interface INonSyntacticToken : IWhiteToken, INode
	{
		IWhiteToken Trailing { get; set; }
	}
}
