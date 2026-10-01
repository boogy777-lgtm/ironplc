using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IPragmaToken : INonSyntacticToken, IWhiteToken, INode
	{
		[Nullable(1)]
		string Pragma
		{
			[NullableContext(1)]
			get;
		}
	}
}
