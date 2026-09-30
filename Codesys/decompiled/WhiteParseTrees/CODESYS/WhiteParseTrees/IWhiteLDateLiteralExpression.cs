using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteLDateLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		long Value { get; }

		[Nullable(1)]
		ILDateToken LDateToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
