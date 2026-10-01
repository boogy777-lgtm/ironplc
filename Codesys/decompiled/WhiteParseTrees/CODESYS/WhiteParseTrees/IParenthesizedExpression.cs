using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IParenthesizedExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		ILeftParenthesisToken LeftParenthesis { get; set; }

		IWhiteExpression Expression { get; set; }

		IRightParenthesisToken RightParenthesis { get; set; }
	}
}
