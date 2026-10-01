using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteSingleByteStringLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		string Value { get; }

		ISingleByteStringToken StringToken { get; set; }
	}
}
