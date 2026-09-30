using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteRepeatStatement : WhiteStatement, IWhiteRepeatStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IRepeatToken Repeat { get; set; }

		public IWhiteSequenceStatement Controlled { get; set; }

		public IUntilToken Until { get; set; }

		public IWhiteExpression Condition { get; set; }

		public IEndRepeatToken EndRepeat { get; set; }

		public WhiteRepeatStatement(IRepeatToken repeat, IWhiteSequenceStatement controlled, IUntilToken until, IWhiteExpression condition, IEndRepeatToken endRepeat)
		{
			Repeat = repeat;
			Controlled = controlled;
			Until = until;
			Condition = condition;
			EndRepeat = endRepeat;
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
			yield return Repeat;
			yield return Controlled;
			yield return Until;
			yield return Condition;
			yield return EndRepeat;
		}
	}
}
