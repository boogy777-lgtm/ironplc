using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteDeRefAccessExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteExpression BaseExpression { get; set; }

		IDeRefToken DeRefToken { get; set; }
	}
}
