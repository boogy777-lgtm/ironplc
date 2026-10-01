using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteDurationLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		uint Value { get; }

		[Nullable(1)]
		IDurationToken Duration
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
