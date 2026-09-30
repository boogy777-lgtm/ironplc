using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteNamespaceDeclarationStatement : WhiteBaseDeclarationStatement, IWhiteNamespaceDeclarationStatement, IWhitePouDeclarationStatement2, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteDeclarationStatement, IWhitePOU, IWhitePOUSyntax
	{
		public IWhiteSequenceStatement BeforeDeclarationStatements { get; set; }

		public IWhiteDeclarationStatement DeclarationStatement => this;

		public IWhiteSequenceStatement Implementation => new WhiteSequenceStatement();

		public IEnumerable<IWhitePOUSyntax> SubPOUs { get; }

		public IEndPouTypeToken EndPOU => EndNamespace;

		public IEnumerable<IAccessSpecifierToken> Access { get; set; }

		public IEndNamespaceToken EndNamespace { get; set; }

		public WhiteNamespaceDeclarationStatement(IWhiteSequenceStatement beforeDeclaration, IPouTypeToken pouClass, IEnumerable<IAccessSpecifierToken> accessSpecifier, IWhiteExpression nameExpression, IWhiteSequenceStatement declarations, IEnumerable<IWhitePOUSyntax> subPOUs, IEndNamespaceToken endNamespace)
			: base(pouClass, nameExpression, declarations)
		{
			Access = accessSpecifier;
			EndNamespace = endNamespace;
			BeforeDeclarationStatements = beforeDeclaration;
			SubPOUs = subPOUs;
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
			yield return base.Class;
			foreach (IAccessSpecifierToken item in Access)
			{
				yield return item;
			}
			yield return base.NameExpression;
			yield return base.Declarations;
			foreach (IWhitePOUSyntax subPOU in SubPOUs)
			{
				yield return subPOU;
			}
			yield return EndNamespace;
		}
	}
}
