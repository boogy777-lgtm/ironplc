using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteParenthesizedExpression : WhiteExpression, IParenthesizedExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public ILeftParenthesisToken LeftParenthesis { get; set; }

		public IWhiteExpression Expression { get; set; }

		public IRightParenthesisToken RightParenthesis { get; set; }

		public WhiteParenthesizedExpression(ILeftParenthesisToken leftParenthesis, IWhiteExpression expression, IRightParenthesisToken rightParenthesis)
		{
			LeftParenthesis = leftParenthesis;
			Expression = expression;
			RightParenthesis = rightParenthesis;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return LeftParenthesis;
			yield return Expression;
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
