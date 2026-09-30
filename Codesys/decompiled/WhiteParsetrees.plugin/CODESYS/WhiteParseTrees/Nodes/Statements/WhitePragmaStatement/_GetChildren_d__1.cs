using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhitePragmaStatement : WhiteStatement, IWhitePragmaStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IPragmaToken PragmaToken { get; set; }

		public WhitePragmaStatement(IPragmaToken token)
		{
			PragmaToken = token;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return PragmaToken;
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
