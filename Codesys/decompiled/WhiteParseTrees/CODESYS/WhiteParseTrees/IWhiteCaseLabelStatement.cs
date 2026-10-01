using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteCaseLabelStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IEnumerable<IWhiteExpression> CaseExpressionList { get; set; }

		IColonToken Colon { get; set; }
	}
}
