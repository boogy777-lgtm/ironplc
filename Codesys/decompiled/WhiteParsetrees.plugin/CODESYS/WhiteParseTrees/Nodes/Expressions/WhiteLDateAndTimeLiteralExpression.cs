using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteLDateAndTimeLiteralExpression : WhiteExpression, IWhiteLDateAndTimeLiteralExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public long Value => LDateAndTimeToken.Value;

		public ILDateAndTimeToken LDateAndTimeToken { get; set; }

		public WhiteLDateAndTimeLiteralExpression(ILDateAndTimeToken token)
		{
			LDateAndTimeToken = token;
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
			yield return LDateAndTimeToken;
		}
	}
}
