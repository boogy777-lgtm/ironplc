using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteFunction : WhiteStatement, IWhiteFunction, IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IWhiteSequenceStatement BeforeDeclarationStatements { get; }

		public IWhiteDeclarationStatement DeclarationStatement => (IWhitePouDeclarationStatement2)Declaration;

		public IWhiteSequenceStatement Implementation { get; }

		public IEnumerable<IWhitePOUSyntax> SubPOUs => new List<IWhitePOUSyntax>();

		public IEndPouTypeToken EndPOU => EndPOUToken;

		public IWhiteFunctionDeclarationStatement Declaration { get; set; }

		public IEndFunctionToken2 EndPOUToken { get; set; }

		public WhiteFunction(IWhiteSequenceStatement seqBefore, IWhiteFunctionDeclarationStatement declaration, IWhiteSequenceStatement implementation, IEndFunctionToken2 endPOUToken)
		{
			BeforeDeclarationStatements = seqBefore;
			Declaration = declaration;
			Implementation = implementation;
			EndPOUToken = endPOUToken;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return BeforeDeclarationStatements;
			yield return Declaration;
			yield return Implementation;
			yield return EndPOUToken;
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
