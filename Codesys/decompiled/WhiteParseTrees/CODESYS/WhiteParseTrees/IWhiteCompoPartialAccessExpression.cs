using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteCompoPartialAccessExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteExpression LeftExpression { get; set; }

		IPeriodToken Period { get; set; }

		IWhitePartialAccessExpression RightExpression { get; set; }
	}
}
