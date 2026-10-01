using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(2)]
	[ReleasedInterface]
	public interface IStringTypeExpression : IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[Nullable(1)]
		IAnyStringSimpleTypeToken StringSimpleOperator
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}

		IAnyBraceLeftToken LeftParenthesis { get; set; }

		IWhiteExpression Length { get; set; }

		IAnyBraceRightToken RightParenthesis { get; set; }
	}
}
