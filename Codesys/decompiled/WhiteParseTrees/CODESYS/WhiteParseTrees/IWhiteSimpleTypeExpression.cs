using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteSimpleTypeExpression : IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[Nullable(1)]
		IWhiteSimpleTypeToken SimpleTypeOperator
		{
			[NullableContext(1)]
			get;
		}
	}
}
