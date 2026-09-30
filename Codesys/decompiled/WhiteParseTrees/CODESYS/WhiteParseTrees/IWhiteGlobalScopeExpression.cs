using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteGlobalScopeExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IPeriodToken Period { get; set; }

		IWhiteVariableExpression Right { get; set; }
	}
}
