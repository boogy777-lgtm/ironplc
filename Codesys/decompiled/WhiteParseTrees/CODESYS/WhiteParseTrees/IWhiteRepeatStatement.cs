using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteRepeatStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IRepeatToken Repeat { get; set; }

		IWhiteSequenceStatement Controlled { get; set; }

		IUntilToken Until { get; set; }

		IWhiteExpression Condition { get; set; }

		IEndRepeatToken EndRepeat { get; set; }
	}
}
