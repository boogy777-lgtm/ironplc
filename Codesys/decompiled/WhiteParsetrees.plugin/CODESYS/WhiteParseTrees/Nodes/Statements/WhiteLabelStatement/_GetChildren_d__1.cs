using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteLabelStatement : WhiteStatement, IWhiteLabelStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public string Label => LabelToken.Identifier;

		public IIdentifierToken LabelToken { get; set; }

		public IColonToken Colon { get; set; }

		internal WhiteLabelStatement(IIdentifierToken labelToken, IColonToken colonToken)
		{
			LabelToken = labelToken;
			Colon = colonToken;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return LabelToken;
			yield return Colon;
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
