using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IEnumerationTypeExpression : IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		ILeftParenthesisToken LeftParenthesis { get; set; }

		IEnumerable<IWhiteExpression> Definitions { get; set; }

		IRightParenthesisToken RightParenthesis { get; set; }
	}
}
