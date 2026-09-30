using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhitePropertyAccessorStatement : WhiteStatement, IWhitePropertyAccessorStatement, IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IWhiteSequenceStatement BeforeDeclarationStatements { get; set; }

		public IWhiteDeclarationStatement DeclarationStatement => Declaration;

		public IWhitePropertyAccessorDeclarationStatement Declaration { get; set; }

		public IWhiteSequenceStatement Implementation { get; set; }

		public IEnumerable<IWhitePOUSyntax> SubPOUs => new List<IWhitePOUSyntax>();

		public IEndPouTypeToken EndPOU => EndProperty;

		public IEndPropertyToken2 EndProperty { get; set; }

		internal WhitePropertyAccessorStatement(IWhiteSequenceStatement before, IWhitePropertyAccessorDeclarationStatement declaration, IWhiteSequenceStatement implementation, IEndPropertyToken2 endProperty)
		{
			BeforeDeclarationStatements = before;
			Declaration = declaration;
			Implementation = implementation;
			EndProperty = endProperty;
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

		public override IEnumerable<INode> GetChildren()
		{
			yield return BeforeDeclarationStatements;
			yield return Declaration;
			yield return Implementation;
			yield return EndProperty;
		}
	}
}
