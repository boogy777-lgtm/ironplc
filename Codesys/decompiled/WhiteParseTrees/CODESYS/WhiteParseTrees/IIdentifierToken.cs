using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IIdentifierToken : IWhiteToken, INode
	{
		[Nullable(1)]
		string Identifier
		{
			[NullableContext(1)]
			get;
		}
	}
}
