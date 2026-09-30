using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteErrorStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IEnumerable<IWhiteToken> TokenList
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
