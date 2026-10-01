using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IExpressionSyntax2 : IExpressionSyntax
	{
		public interface IExpressionVisitor2 : IExpressionVisitor
		{
			void visit(IWhitePartialAccessExpression expression);

			void visit(IWhiteCompoPartialAccessExpression expression);
		}

		public interface IExpressionVisitor2<[Nullable(2)] out T> : IExpressionVisitor<T>
		{
			T visit(IWhitePartialAccessExpression expression);

			T visit(IWhiteCompoPartialAccessExpression expression);
		}

		public interface IExpressionVisitor2<[Nullable(2)] out T, [Nullable(2)] in TContext> : IExpressionVisitor<T, TContext>
		{
			T visit(IWhitePartialAccessExpression expression, TContext context);

			T visit(IWhiteCompoPartialAccessExpression expression, TContext context);
		}
	}
}
