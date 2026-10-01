using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteDoubleByteStringLiteralExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		string Value { get; }

		IDoubleByteStringToken StringToken { get; set; }
	}
}
