using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteDateAndTimeLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		DateTime Date { get; }

		[Nullable(1)]
		IDateAndTimeToken DateAndTimeToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
