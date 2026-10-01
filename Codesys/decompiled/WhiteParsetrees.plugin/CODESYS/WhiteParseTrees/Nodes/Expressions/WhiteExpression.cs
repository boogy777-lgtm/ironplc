using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	public abstract class WhiteExpression : WhiteExprement, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public abstract void Accept(IExpressionSyntax.IExpressionVisitor visitor);

		[System.Runtime.CompilerServices.NullableContext(1)]
		public abstract T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IExpressionSyntax.IExpressionVisitor<T> visitor);

		[System.Runtime.CompilerServices.NullableContext(1)]
		public abstract T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IExpressionSyntax.IExpressionVisitor<T, TContext> visitor, TContext context);
	}
}
