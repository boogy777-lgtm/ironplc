using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteArrayInitializationExpression : WhiteExpression, IWhiteArrayInitializationExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public ILeftBracketToken LeftBracket { get; set; }

		public IEnumerable<IWhiteExpression> InitValues { get; set; }

		public IRightBracketToken RightBracket { get; set; }

		internal WhiteArrayInitializationExpression(ILeftBracketToken leftBracket, IEnumerable<IWhiteExpression> initValues, IRightBracketToken rightBracket)
		{
			LeftBracket = leftBracket;
			InitValues = initValues;
			RightBracket = rightBracket;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return LeftBracket;
			foreach (IWhiteExpression initValue in InitValues)
			{
				yield return initValue;
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
