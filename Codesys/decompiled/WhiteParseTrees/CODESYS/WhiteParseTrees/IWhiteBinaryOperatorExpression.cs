using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteBinaryOperatorExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteExpression First { get; set; }

		IWhiteOperatorToken OperatorToken { get; set; }

		IWhiteExpression Second { get; set; }
	}
}
