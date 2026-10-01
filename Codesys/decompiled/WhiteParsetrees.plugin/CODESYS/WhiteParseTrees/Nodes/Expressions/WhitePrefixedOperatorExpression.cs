using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhitePrefixedOperatorExpression : WhiteExpression, IWhitePrefixedOperatorExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhitePrefixedOperatorToken Operator { get; set; }

		public ILeftParenthesisToken LeftParenthesis { get; set; }

		public IEnumerable<IWhiteExpression> Operands { get; set; }

		public IRightParenthesisToken RightParenthesis { get; set; }

		public WhitePrefixedOperatorExpression(IWhitePrefixedOperatorToken @operator, ILeftParenthesisToken leftParenthesis, IEnumerable<IWhiteExpression> operands, IRightParenthesisToken rightParenthesis)
		{
			Operator = @operator;
			LeftParenthesis = leftParenthesis;
			Operands = operands;
			RightParenthesis = rightParenthesis;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Operator;
			yield return LeftParenthesis;
			foreach (IWhiteExpression operand in Operands)
			{
				yield return operand;
			}
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
