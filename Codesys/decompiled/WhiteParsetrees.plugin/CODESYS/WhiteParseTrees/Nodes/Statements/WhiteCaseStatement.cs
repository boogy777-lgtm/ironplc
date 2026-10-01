using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteCaseStatement : WhiteStatement, IWhiteCaseStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public ICaseToken Case { get; set; }

		public IWhiteExpression Switch { get; set; }

		public IOfToken Of { get; set; }

		public IEnumerable<IWhiteCase> Cases { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IElseToken ElseToken
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteSequenceStatement Else
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IEndCaseToken EndCase { get; set; }

		public WhiteCaseStatement(ICaseToken _case, IWhiteExpression _switch, IOfToken _of, IEnumerable<IWhiteCase> cases, [System.Runtime.CompilerServices.Nullable(2)] IElseToken elsetoken, [System.Runtime.CompilerServices.Nullable(2)] IWhiteSequenceStatement _else, IEndCaseToken endcase)
		{
			Case = _case;
			Switch = _switch;
			Of = _of;
			Cases = cases;
			ElseToken = elsetoken;
			Else = _else;
			EndCase = endcase;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Case;
			yield return Switch;
			yield return Of;
			foreach (IWhiteCase _case in Cases)
			{
				yield return _case.Label;
				yield return _case.Controlled;
			}
			if (ElseToken != null)
			{
				yield return ElseToken;
			}
			if (Else != null)
			{
				yield return Else;
			}
			yield return EndCase;
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
	}
}
