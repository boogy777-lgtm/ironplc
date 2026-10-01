using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteVariableDeclarationListStatement : WhiteStatement, IWhiteVariableDeclarationListStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IVarListStartToken BeginVarOp { get; set; }

		public IEnumerable<IWhiteOperatorToken> PersistantRetain { get; set; }

		public IWhiteSequenceStatement Declarations { get; set; }

		public IVarListEndToken EndVarOp { get; set; }

		public WhiteVariableDeclarationListStatement(IVarListStartToken beginVarOp, IEnumerable<IWhiteOperatorToken> persistantRetain, IWhiteSequenceStatement declarations, IVarListEndToken endVarOp)
		{
			BeginVarOp = beginVarOp;
			PersistantRetain = persistantRetain;
			Declarations = declarations;
			EndVarOp = endVarOp;
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
			yield return BeginVarOp;
			foreach (IWhiteOperatorToken item in PersistantRetain)
			{
				yield return item;
			}
			yield return Declarations;
			yield return EndVarOp;
		}
	}
}
