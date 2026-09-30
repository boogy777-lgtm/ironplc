using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	public abstract class WhiteStatement : WhiteExprement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public abstract void Accept(IStatementSyntax.IStatementVisitor visitor);

		[System.Runtime.CompilerServices.NullableContext(1)]
		public abstract T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IStatementSyntax.IStatementVisitor<T> visitor);

		[System.Runtime.CompilerServices.NullableContext(1)]
		public abstract T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IStatementSyntax.IStatementVisitor<T, TContext> visitor, TContext context);
	}
}
