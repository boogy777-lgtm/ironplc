using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteXStringLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		string Value { get; }

		IXByteStringToken StringToken { get; set; }
	}
}
