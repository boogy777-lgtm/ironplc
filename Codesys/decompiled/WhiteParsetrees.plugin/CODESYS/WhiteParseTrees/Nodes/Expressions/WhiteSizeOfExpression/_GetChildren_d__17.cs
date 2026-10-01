using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteSizeOfExpression : WhiteExpression, IWhiteSizeOfExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhiteOperatorToken SizeOf { get; set; }

		public ILeftParenthesisToken LeftParenthesis { get; set; }

		public IWhiteTypeExpression TypeExpression { get; set; }

		public IRightParenthesisToken RightParenthesis { get; set; }

		public WhiteSizeOfExpression(IWhiteOperatorToken sizeofOperator, ILeftParenthesisToken leftParenthesis, IWhiteTypeExpression typeExpression, IRightParenthesisToken rightParenthesis)
		{
			SizeOf = sizeofOperator;
			LeftParenthesis = leftParenthesis;
			TypeExpression = typeExpression;
			RightParenthesis = rightParenthesis;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return SizeOf;
			yield return LeftParenthesis;
			yield return TypeExpression;
			yield return RightParenthesis;
		}

		public override void Accept(IExpressionSyntax.IExpressionVisitor visitor)
		{
			visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IExpressionSyntax.IExpressionVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IExpressionSyntax.IExpressionVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}
	}
}
