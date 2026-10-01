using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface ILeadByCommaExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		ICommaToken Comma { get; set; }

		IWhiteExpression Expression { get; set; }
	}
}
