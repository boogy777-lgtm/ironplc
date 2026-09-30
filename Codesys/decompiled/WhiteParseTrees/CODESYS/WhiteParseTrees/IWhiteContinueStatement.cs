using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteContinueStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IContinueToken Continue { get; set; }

		ISemicolonToken Semicolon { get; set; }
	}
}
