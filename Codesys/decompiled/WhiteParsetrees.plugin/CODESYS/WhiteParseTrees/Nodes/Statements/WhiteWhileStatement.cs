using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteWhileStatement : WhiteStatement, IWhiteWhileStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IWhileToken While { get; set; }

		public IWhiteExpression Condition { get; set; }

		public IDoToken Do { get; set; }

		public IWhiteSequenceStatement Controlled { get; set; }

		public IEndWhileToken EndWhile { get; set; }

		public WhiteWhileStatement(IWhileToken @while, IWhiteExpression condition, IDoToken @do, IWhiteSequenceStatement controlled, IEndWhileToken endWhile)
		{
			While = @while;
			Condition = condition;
			Do = @do;
			Controlled = controlled;
			EndWhile = endWhile;
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
			yield return While;
			yield return Condition;
			yield return Do;
			yield return Controlled;
			yield return EndWhile;
		}
	}
}
