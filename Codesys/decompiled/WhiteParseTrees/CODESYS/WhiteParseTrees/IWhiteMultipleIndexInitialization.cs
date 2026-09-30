using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteMultipleIndexInitialization : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteExpression Number { get; set; }

		IWhiteExpression Value { get; set; }
	}
}
