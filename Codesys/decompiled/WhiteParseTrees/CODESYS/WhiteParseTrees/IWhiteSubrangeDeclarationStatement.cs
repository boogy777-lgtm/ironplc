using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteSubrangeDeclarationStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IEnumerable<IWhiteExpression> VariableNames { get; set; }

		IColonToken Colon { get; set; }

		ISubRangeTypeExpression SubRangeType { get; set; }

		ISemicolonToken Semicolon { get; set; }
	}
}
