using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteForStatement : WhiteStatement, IWhiteForStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IForToken For { get; set; }

		public IWhiteAssignmentExpression StartExpression { get; set; }

		public IToToken To { get; set; }

		public IWhiteExpression UpperBound { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IByToken By
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExpression StepWidth
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IDoToken Do { get; set; }

		public IWhiteSequenceStatement Controlled { get; set; }

		public IEndForToken EndFor { get; set; }

		public WhiteForStatement(IForToken @for, IWhiteAssignmentExpression startExpression, IToToken to, IWhiteExpression upperBound, [System.Runtime.CompilerServices.Nullable(2)] IByToken by, [System.Runtime.CompilerServices.Nullable(2)] IWhiteExpression stepWidth, IDoToken @do, IWhiteSequenceStatement controlled, IEndForToken endFor)
		{
			For = @for;
			StartExpression = startExpression;
			To = to;
			UpperBound = upperBound;
			By = by;
			StepWidth = stepWidth;
			Do = @do;
			Controlled = controlled;
			EndFor = endFor;
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
			yield return For;
			yield return StartExpression;
			yield return To;
			yield return UpperBound;
			if (By != null)
			{
				yield return By;
			}
			if (StepWidth != null)
			{
				yield return StepWidth;
			}
			yield return Do;
			yield return Controlled;
			yield return EndFor;
		}
	}
}
