using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteNamespaceAccessExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IWhiteExpression Namespace { get; set; }

		IHashToken HashTag { get; set; }

		IWhiteVariableExpression VariableExpression { get; set; }
	}
}
