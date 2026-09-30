using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface ISubRangeTypeExpression : IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteTypeExpression BaseType { get; set; }

		ILeftParenthesisToken LeftParenthesis { get; set; }

		IWhiteRangeExpression Range { get; set; }

		IRightParenthesisToken RightParenthesis { get; set; }
	}
}
