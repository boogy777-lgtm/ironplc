using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteElseIfStatement : WhiteStatement, IWhiteElseIfStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IElseIfToken ElseIf { get; set; }

		public IWhiteExpression Condition { get; set; }

		public IThenToken Then { get; set; }

		public IWhiteSequenceStatement ThenStatement { get; set; }

		public WhiteElseIfStatement(IElseIfToken elseIf, IWhiteExpression condition, IThenToken then, IWhiteSequenceStatement thenStatement)
		{
			ElseIf = elseIf;
			Condition = condition;
			Then = then;
			ThenStatement = thenStatement;
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
			yield return ElseIf;
			yield return Condition;
			yield return Then;
			yield return ThenStatement;
		}
	}
}
