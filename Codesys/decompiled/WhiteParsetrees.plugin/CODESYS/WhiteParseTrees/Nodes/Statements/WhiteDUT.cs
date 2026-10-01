using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteDUT : WhiteStatement, IWhiteDUT, IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IWhiteTypeDeclarationStatement TypeDeclarationStatement { get; set; }

		public IEndTypeToken EndTypeToken { get; set; }

		public IWhiteSequenceStatement BeforeDeclarationStatements { get; }

		public IWhiteDeclarationStatement DeclarationStatement => TypeDeclarationStatement;

		public IWhiteSequenceStatement Implementation => new WhiteSequenceStatement();

		public IEnumerable<IWhitePOUSyntax> SubPOUs => new List<IWhitePOUSyntax>();

		public IEndPouTypeToken EndPOU => (IEndPouTypeToken)EndTypeToken;

		public WhiteDUT(IWhiteSequenceStatement seqBefore, IWhiteTypeDeclarationStatement typeDeclaration, IEndTypeToken endTypeToken)
		{
			BeforeDeclarationStatements = seqBefore;
			TypeDeclarationStatement = typeDeclaration;
			EndTypeToken = endTypeToken;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return BeforeDeclarationStatements;
			yield return TypeDeclarationStatement;
			yield return Implementation;
		}

		public override void Accept(IStatementSyntax.IStatementVisitor visitor)
		{
			if (visitor is IStatementSyntax2.IStatementVisitor2 statementVisitor)
			{
				statementVisitor.visit(this);
			}
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IStatementSyntax.IStatementVisitor<T> visitor)
		{
			if (visitor is IStatementSyntax2.IStatementVisitor2<T> statementVisitor)
			{
				return statementVisitor.visit(this);
			}
			return default(T);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IStatementSyntax.IStatementVisitor<T, TContext> visitor, TContext context)
		{
			if (visitor is IStatementSyntax2.IStatementVisitor2<T, TContext> statementVisitor)
			{
				return statementVisitor.visit(this, context);
			}
			return default(T);
		}
	}
}
