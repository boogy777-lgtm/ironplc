using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteToken : INode
	{
		[Nullable(2)]
		IWhiteToken Leading
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		WhiteTokenType Type { get; }

		string Text { get; set; }
	}
}
