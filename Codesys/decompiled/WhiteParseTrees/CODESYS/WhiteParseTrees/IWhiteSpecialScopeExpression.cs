using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteSpecialScopeExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteScopeToken Scope { get; set; }

		IPeriodToken Period { get; set; }

		IWhiteExpression Right { get; set; }
	}
}
