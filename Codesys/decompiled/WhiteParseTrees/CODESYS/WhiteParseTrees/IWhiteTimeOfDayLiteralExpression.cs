using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteTimeOfDayLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		DateTime Date { get; }

		[Nullable(1)]
		ITimeOfDayToken TimeOfDayToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
