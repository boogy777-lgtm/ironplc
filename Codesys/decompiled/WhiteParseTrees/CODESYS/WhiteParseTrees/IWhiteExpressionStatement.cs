using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteExpressionStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IWhiteExpression Expr { get; set; }

		ISemicolonToken Semicolon { get; set; }
	}
}
