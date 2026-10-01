using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhitePragmaIfStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IWhitePragmaStatement If { get; set; }

		IWhiteSequenceStatement ThenStatement { get; set; }

		IEnumerable<IWhitePragmaElseIfStatement> ElseIfStatement { get; set; }

		[Nullable(2)]
		IWhitePragmaStatement Else
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IWhiteSequenceStatement ElseStatement
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IWhitePragmaStatement EndIf { get; set; }
	}
}
