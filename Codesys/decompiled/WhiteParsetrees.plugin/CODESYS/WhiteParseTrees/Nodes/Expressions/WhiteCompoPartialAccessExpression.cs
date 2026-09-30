using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	public class WhiteCompoPartialAccessExpression : WhiteExpression, IWhiteCompoPartialAccessExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhiteExpression LeftExpression { get; set; }

		public IPeriodToken Period { get; set; }

		public IWhitePartialAccessExpression RightExpression { get; set; }

		public WhiteCompoPartialAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IWhitePartialAccessExpression rightExpression)
		{
			LeftExpression = leftExpression;
			Period = period;
			RightExpression = rightExpression;
		}

		public override void Accept(IExpressionSyntax.IExpressionVisitor visitor)
		{
			if (visitor is IExpressionSyntax2.IExpressionVisitor2 expressionVisitor)
			{
				expressionVisitor.visit(this);
			}
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IExpressionSyntax.IExpressionVisitor<T> visitor)
		{
			if (visitor is IExpressionSyntax2.IExpressionVisitor2<T> expressionVisitor)
			{
				return expressionVisitor.visit(this);
			}
			return default(T);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IExpressionSyntax.IExpressionVisitor<T, TContext> visitor, TContext context)
		{
			if (visitor is IExpressionSyntax2.IExpressionVisitor2<T, TContext> expressionVisitor)
			{
				return expressionVisitor.visit(this, context);
			}
			return default(T);
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return LeftExpression;
			yield return Period;
			yield return RightExpression;
		}
	}
}
