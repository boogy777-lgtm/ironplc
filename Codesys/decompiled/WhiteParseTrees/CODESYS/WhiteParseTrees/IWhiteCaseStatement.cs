using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteCaseStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		ICaseToken Case { get; set; }

		IWhiteExpression Switch { get; set; }

		IOfToken Of { get; set; }

		IEnumerable<IWhiteCase> Cases { get; set; }

		[Nullable(2)]
		IElseToken ElseToken
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IWhiteSequenceStatement Else
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IEndCaseToken EndCase { get; set; }
	}
}
