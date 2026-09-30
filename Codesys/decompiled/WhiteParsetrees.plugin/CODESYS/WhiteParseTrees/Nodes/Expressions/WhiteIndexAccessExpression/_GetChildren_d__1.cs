using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteIndexAccessExpression : WhiteExpression, IWhiteIndexAccessExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhiteExpression Base { get; set; }

		public ILeftBracketToken LeftBracket { get; set; }

		public IEnumerable<IWhiteExpression> Accesses { get; set; }

		public IRightBracketToken RightBracket { get; set; }

		public WhiteIndexAccessExpression(IWhiteExpression @base, ILeftBracketToken leftBracket, IEnumerable<IWhiteExpression> accesses, IRightBracketToken rightBracket)
		{
			Base = @base;
			LeftBracket = leftBracket;
			Accesses = accesses;
			RightBracket = rightBracket;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Base;
			yield return LeftBracket;
			foreach (IWhiteExpression access in Accesses)
			{
				yield return access;
			}
			yield return RightBracket;
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
