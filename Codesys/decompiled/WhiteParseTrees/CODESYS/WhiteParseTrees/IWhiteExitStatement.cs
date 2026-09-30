using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteExitStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IExitToken Exit { get; set; }

		ISemicolonToken Semicolon { get; set; }
	}
}
