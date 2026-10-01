using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteBinaryOperatorExpression : WhiteExpression, IWhiteBinaryOperatorExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhiteExpression First { get; set; }

		public IWhiteOperatorToken OperatorToken { get; set; }

		public IWhiteExpression Second { get; set; }

		public WhiteBinaryOperatorExpression(IWhiteExpression first, IWhiteOperatorToken token, IWhiteExpression second)
		{
			First = first;
			OperatorToken = token;
			Second = second;
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
			yield return First;
			yield return OperatorToken;
			yield return Second;
		}
	}
}
