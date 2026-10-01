using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteIntegerLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		ulong Value { get; }

		[Nullable(1)]
		IIntegerToken Integer
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
