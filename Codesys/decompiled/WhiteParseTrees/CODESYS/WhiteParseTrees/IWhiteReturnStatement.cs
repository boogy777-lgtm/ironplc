using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteReturnStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IReturnToken Return { get; set; }

		[Nullable(2)]
		IParenthesizedExpression Condition
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		ISemicolonToken Semicolon { get; set; }
	}
}
