using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteSuperExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[Nullable(1)]
		ISuperToken Super
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
