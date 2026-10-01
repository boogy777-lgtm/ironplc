using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteBoolLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		bool Value { get; }

		[Nullable(1)]
		IBooleanToken Boolean
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
