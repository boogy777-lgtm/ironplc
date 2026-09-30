using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteElseIfStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IElseIfToken ElseIf { get; set; }

		IWhiteExpression Condition { get; set; }

		IThenToken Then { get; set; }

		IWhiteSequenceStatement ThenStatement { get; set; }
	}
}
