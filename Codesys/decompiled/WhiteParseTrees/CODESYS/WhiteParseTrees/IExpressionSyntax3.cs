using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IExpressionSyntax3 : IExpressionSyntax2, IExpressionSyntax
	{
		public interface IExpressionVisitor3 : IExpressionVisitor2, IExpressionVisitor
		{
			[NullableContext(1)]
			void visit(IWhiteVariableArrayTypeExpression expression);
		}

		[NullableContext(1)]
		public interface IExpressionVisitor3<[Nullable(2)] out T> : IExpressionVisitor2<T>, IExpressionVisitor<T>
		{
			T visit(IWhiteVariableArrayTypeExpression expression);
		}

		[NullableContext(2)]
		public interface IExpressionVisitor3<out T, in TContext> : IExpressionVisitor2<T, TContext>, IExpressionVisitor<T, TContext>
		{
			[NullableContext(1)]
			T visit(IWhiteVariableArrayTypeExpression expression, TContext ctx);
		}
	}
}
