using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteVariableExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		string Identifier { get; }

		IIdentifierToken IdentToken { get; set; }
	}
}
