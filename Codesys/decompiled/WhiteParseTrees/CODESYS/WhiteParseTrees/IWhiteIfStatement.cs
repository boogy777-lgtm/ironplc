using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteIfStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IIfToken If { get; set; }

		IWhiteExpression Condition { get; set; }

		IThenToken Then { get; set; }

		IWhiteSequenceStatement ThenStatement { get; set; }

		IEnumerable<IWhiteElseIfStatement> ElseIfStatement { get; set; }

		[Nullable(2)]
		IElseToken Else
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

		IEndIfToken EndIf { get; set; }
	}
}
