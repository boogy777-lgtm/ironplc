using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Services.Formatter
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class StandardExpressionTraverser
	{
		private IExpressionSyntax.IExpressionVisitor<bool, int> Visitor { get; }

		public StandardExpressionTraverser(IExpressionSyntax.IExpressionVisitor<bool, int> visitor)
		{
			Visitor = visitor;
		}

		public void VisitAllExpressions(INode node, int indentation)
		{
			if (node is IWhiteErrorStatement)
			{
				return;
			}
			if (node is IWhiteExpression whiteExpression)
			{
				whiteExpression.Accept(Visitor, indentation);
			}
			if (node is IWhiteCallExpression)
			{
				indentation++;
			}
			foreach (INode child in node.GetChildren())
			{
				if (child is IWhiteToken whiteToken && whiteToken.Leading != null)
				{
					VisitAllExpressions(whiteToken.Leading, indentation);
				}
				VisitAllExpressions(child, indentation);
			}
		}
	}
}
