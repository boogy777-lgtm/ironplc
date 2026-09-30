using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteJumpStatement : WhiteStatement, IWhiteJumpStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IJumpToken Jump { get; set; }

		public ISemicolonToken Semicolon { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IParenthesizedExpression Condition
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IIdentifierToken LabelToken { get; set; }

		public string Label => LabelToken.Identifier;

		public WhiteJumpStatement(IJumpToken tokenJump, [System.Runtime.CompilerServices.Nullable(2)] IParenthesizedExpression condition, IIdentifierToken labelToken, ISemicolonToken semicolon)
		{
			Jump = tokenJump;
			Condition = condition;
			LabelToken = labelToken;
			Semicolon = semicolon;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Jump;
			if (Condition != null)
			{
				yield return Condition;
			}
			yield return LabelToken;
			yield return Semicolon;
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
