using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteExitStatement : WhiteStatement, IWhiteExitStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public ISemicolonToken Semicolon { get; set; }

		public IExitToken Exit { get; set; }

		public WhiteExitStatement(IExitToken exitToken, ISemicolonToken semicolon)
		{
			Exit = exitToken;
			Semicolon = semicolon;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Exit;
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
