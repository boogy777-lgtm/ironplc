using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteCallExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteExpression Callee { get; set; }

		ILeftParenthesisToken LeftParenthesis { get; set; }

		IEnumerable<IWhiteExpression> Params { get; set; }

		IRightParenthesisToken RightParenthesis { get; set; }
	}
}
