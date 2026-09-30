using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteCallExpression : WhiteExpression, IWhiteCallExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhiteExpression Callee { get; set; }

		public ILeftParenthesisToken LeftParenthesis { get; set; }

		public IEnumerable<IWhiteExpression> Params { get; set; }

		public IRightParenthesisToken RightParenthesis { get; set; }

		internal WhiteCallExpression(IWhiteExpression callee, ILeftParenthesisToken leftParenthesis, IEnumerable<IWhiteExpression> @params, IRightParenthesisToken rightParenthesis)
		{
			Callee = callee;
			LeftParenthesis = leftParenthesis;
			Params = @params;
			RightParenthesis = rightParenthesis;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Callee;
			yield return LeftParenthesis;
			foreach (IWhiteExpression param in Params)
			{
				yield return param;
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
