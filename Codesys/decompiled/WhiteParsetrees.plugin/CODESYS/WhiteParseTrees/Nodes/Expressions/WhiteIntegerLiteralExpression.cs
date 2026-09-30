using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteIntegerLiteralExpression : WhiteExpression, IWhiteIntegerLiteralExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public ulong Value => Integer.Value;

		public IIntegerToken Integer { get; set; }

		public WhiteIntegerLiteralExpression(IIntegerToken intToken)
		{
			Integer = intToken;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Integer;
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
