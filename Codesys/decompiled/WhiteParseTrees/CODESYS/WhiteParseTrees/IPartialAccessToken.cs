using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IPartialAccessToken : IWhiteToken, INode
	{
		[Nullable(1)]
		object Overflow
		{
			[NullableContext(1)]
			get;
		}

		int Offset { get; }

		DirectVariableSize Size { get; }
	}
}
