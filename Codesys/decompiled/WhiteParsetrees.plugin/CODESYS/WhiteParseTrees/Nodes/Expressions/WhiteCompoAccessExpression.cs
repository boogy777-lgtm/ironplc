using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteCompoAccessExpression : WhiteExpression, IWhiteCompoAccessExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhiteExpression LeftExpression { get; set; }

		public IPeriodToken Period { get; set; }

		public IWhiteVariableExpression RightExpression { get; set; }

		public WhiteCompoAccessExpression(IWhiteExpression leftExpression, IPeriodToken period, IWhiteVariableExpression rightExpression)
		{
			LeftExpression = leftExpression;
			Period = period;
			RightExpression = rightExpression;
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

		public override IEnumerable<INode> GetChildren()
		{
			yield return LeftExpression;
			yield return Period;
			yield return RightExpression;
		}
	}
}
