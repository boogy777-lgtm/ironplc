using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ISingleByteStringToken : IWhiteToken, INode
	{
		[Nullable(1)]
		string MyString
		{
			[NullableContext(1)]
			get;
		}
	}
}
