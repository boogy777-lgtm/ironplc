using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhitePrefixedOperatorExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhitePrefixedOperatorToken Operator { get; set; }

		ILeftParenthesisToken LeftParenthesis { get; set; }

		IEnumerable<IWhiteExpression> Operands { get; set; }

		IRightParenthesisToken RightParenthesis { get; set; }
	}
}
