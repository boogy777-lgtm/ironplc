using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	public class WhiteEmptyExpression : WhiteExpression, IEmptyExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public override IEnumerable<INode> GetChildren()
		{
			return new INode[0];
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
