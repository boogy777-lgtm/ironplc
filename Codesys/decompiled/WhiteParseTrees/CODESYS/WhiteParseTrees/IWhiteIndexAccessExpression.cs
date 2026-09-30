using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteIndexAccessExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteExpression Base { get; set; }

		ILeftBracketToken LeftBracket { get; set; }

		IEnumerable<IWhiteExpression> Accesses { get; set; }

		IRightBracketToken RightBracket { get; set; }
	}
}
