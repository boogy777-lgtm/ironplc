using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteGlobalScopeExpression : WhiteExpression, IWhiteGlobalScopeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IPeriodToken Period { get; set; }

		public IWhiteVariableExpression Right { get; set; }

		internal WhiteGlobalScopeExpression(IPeriodToken period, IWhiteVariableExpression right)
		{
			Period = period;
			Right = right;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Period;
			yield return Right;
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
