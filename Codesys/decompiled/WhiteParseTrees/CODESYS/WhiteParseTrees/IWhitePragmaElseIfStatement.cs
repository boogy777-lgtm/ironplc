using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhitePragmaElseIfStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IWhitePragmaStatement ElseIf { get; set; }

		IWhiteSequenceStatement ThenStatement { get; set; }
	}
}
