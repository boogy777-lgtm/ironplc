using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteUnaryOperatorExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		Operator KindOf { get; }

		IWhiteOperatorToken OperatorToken { get; set; }

		IWhiteExpression Operand { get; set; }
	}
}
