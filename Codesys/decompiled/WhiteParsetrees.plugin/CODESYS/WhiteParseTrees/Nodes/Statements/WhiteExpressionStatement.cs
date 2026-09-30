using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteExpressionStatement : WhiteStatement, IWhiteExpressionStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IWhiteExpression Expr { get; set; }

		public ISemicolonToken Semicolon { get; set; }

		public WhiteExpressionStatement(IWhiteExpression whiteExpression, ISemicolonToken semicolon)
		{
			Expr = whiteExpression;
			Semicolon = semicolon;
		}

		public override void Accept(IStatementSyntax.IStatementVisitor visitor)
		{
			visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IStatementSyntax.IStatementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IStatementSyntax.IStatementVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Expr;
			yield return Semicolon;
		}
	}
}
