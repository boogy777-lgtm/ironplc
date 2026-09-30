using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteLDurationLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		ulong Value { get; }

		[Nullable(1)]
		ILDurationToken LDurationToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
