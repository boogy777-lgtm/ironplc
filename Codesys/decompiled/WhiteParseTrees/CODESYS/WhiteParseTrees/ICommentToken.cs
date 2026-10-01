using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ICommentToken : INonSyntacticToken, IWhiteToken, INode
	{
		[Nullable(1)]
		string Comment
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}

		bool IsBlockComment { get; }
	}
}
