using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteErrorPOU : IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IWhiteErrorStatement ErrorStatement
		{
			[NullableContext(1)]
			get;
		}
	}
}
