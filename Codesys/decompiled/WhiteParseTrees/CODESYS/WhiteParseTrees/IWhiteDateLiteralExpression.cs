using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteDateLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		DateTime Value { get; }

		[Nullable(1)]
		IDateToken DateToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
