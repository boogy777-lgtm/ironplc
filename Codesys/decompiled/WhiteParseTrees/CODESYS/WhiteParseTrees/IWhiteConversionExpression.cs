using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteConversionExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IConversionToken ConversionOperator { get; set; }

		ILeftParenthesisToken LeftParenthesis { get; set; }

		IWhiteExpression Expression { get; set; }

		IRightParenthesisToken RightParenthesis { get; set; }

		TypeClass From { get; }

		TypeClass To { get; }
	}
}
