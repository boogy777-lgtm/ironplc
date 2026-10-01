using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(2)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class StringTypeExpression : WhiteTypeExpression, IStringTypeExpression, IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		public IAnyStringSimpleTypeToken StringSimpleOperator
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
			[System.Runtime.CompilerServices.NullableContext(1)]
			set;
		}

		public IAnyBraceLeftToken LeftParenthesis { get; set; }

		public IWhiteExpression Length { get; set; }

		public IAnyBraceRightToken RightParenthesis { get; set; }

		public override TypeClass Class => TypeClass.String;

		public StringTypeExpression([System.Runtime.CompilerServices.Nullable(1)] IAnyStringSimpleTypeToken stringSimpleOperator, IAnyBraceLeftToken leftParenthesis, IWhiteExpression length, IAnyBraceRightToken rightParenthesis)
		{
			StringSimpleOperator = stringSimpleOperator;
			LeftParenthesis = leftParenthesis;
			Length = length;
			RightParenthesis = rightParenthesis;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override IEnumerable<INode> GetChildren()
		{
			yield return StringSimpleOperator;
			if (LeftParenthesis != null)
			{
				yield return LeftParenthesis;
			}
			if (Length != null)
			{
				yield return Length;
			}
			if (RightParenthesis != null)
			{
				yield return RightParenthesis;
			}
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override void Accept(IExpressionSyntax.IExpressionVisitor visitor)
		{
			visitor.visit(this);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IExpressionSyntax.IExpressionVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IExpressionSyntax.IExpressionVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}
	}
}
