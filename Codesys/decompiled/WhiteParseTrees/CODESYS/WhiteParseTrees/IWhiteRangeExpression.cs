using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteRangeExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteExpression Low { get; set; }

		IRangeToken Range { get; set; }

		IWhiteExpression High { get; set; }
	}
}
