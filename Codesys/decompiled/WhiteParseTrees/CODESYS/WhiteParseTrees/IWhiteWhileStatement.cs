using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteWhileStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IWhileToken While { get; set; }

		IWhiteExpression Condition { get; set; }

		IDoToken Do { get; set; }

		IWhiteSequenceStatement Controlled { get; set; }

		IEndWhileToken EndWhile { get; set; }
	}
}
