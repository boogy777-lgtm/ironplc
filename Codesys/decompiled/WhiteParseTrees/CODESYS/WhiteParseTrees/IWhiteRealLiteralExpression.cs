using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteRealLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		double Value { get; }

		[Nullable(1)]
		IRealToken Real
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
