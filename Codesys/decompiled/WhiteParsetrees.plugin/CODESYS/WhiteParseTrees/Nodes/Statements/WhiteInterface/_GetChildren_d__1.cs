using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteInterface : WhiteStatement, IWhiteInterface, IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IWhiteSequenceStatement BeforeDeclarationStatements { get; }

		public IWhiteDeclarationStatement DeclarationStatement => Declaration;

		public IWhiteSequenceStatement Implementation => new WhiteSequenceStatement();

		public IWhiteInterfaceDeclarationStatement3 Declaration { get; set; }

		public IEnumerable<IWhitePOUSyntax> SubPOUs { get; }

		public IEndPouTypeToken EndPOU => EndPOUToken;

		public IEndInterfaceToken2 EndPOUToken { get; set; }

		public WhiteInterface(IWhiteSequenceStatement seqBefore, IWhiteInterfaceDeclarationStatement3 declaration, IEnumerable<IWhitePOUSyntax> subPOUs, IEndInterfaceToken2 endPOUToken)
		{
			BeforeDeclarationStatements = seqBefore;
			Declaration = declaration;
			SubPOUs = subPOUs;
			EndPOUToken = endPOUToken;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return BeforeDeclarationStatements;
			yield return Declaration;
			foreach (IWhitePOUSyntax subPOU in SubPOUs)
			{
				yield return subPOU;
			}
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
