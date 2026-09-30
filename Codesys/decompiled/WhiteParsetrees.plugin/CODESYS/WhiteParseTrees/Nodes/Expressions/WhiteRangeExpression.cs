using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteRangeExpression : WhiteExpression, IWhiteRangeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhiteExpression Low { get; set; }

		public IRangeToken Range { get; set; }

		public IWhiteExpression High { get; set; }

		public WhiteRangeExpression(IWhiteExpression low, IRangeToken range, IWhiteExpression high)
		{
			Low = low;
			Range = range;
			High = high;
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
			yield return Low;
			yield return Range;
			yield return High;
		}
	}
}
