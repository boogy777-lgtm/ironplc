using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteLTimeOfDayLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		long Value { get; }

		[Nullable(1)]
		ILTimeOfDayToken LTimeOfDayToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
