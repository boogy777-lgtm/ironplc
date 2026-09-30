using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteSizeOfExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteOperatorToken SizeOf { get; set; }

		ILeftParenthesisToken LeftParenthesis { get; set; }

		IWhiteTypeExpression TypeExpression { get; set; }

		IRightParenthesisToken RightParenthesis { get; set; }
	}
}
