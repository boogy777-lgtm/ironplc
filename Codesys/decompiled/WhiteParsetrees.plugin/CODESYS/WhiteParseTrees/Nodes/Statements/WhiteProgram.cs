using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteProgram : WhitePouHavingSubPous, IWhiteProgram, IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IWhiteDeclarationStatement DeclarationStatement => (IWhitePouDeclarationStatement2)Declaration;

		public IWhiteProgramDeclarationStatement Declaration { get; set; }

		public IEndPouTypeToken EndPOU => EndPOUToken;

		public IEndProgramToken2 EndPOUToken { get; set; }

		public WhiteProgram(IWhiteSequenceStatement seqBefore, IWhiteProgramDeclarationStatement declaration, IWhiteSequenceStatement implementationAndSubPOUs, IEndProgramToken2 endPOUToken)
			: base(seqBefore, implementationAndSubPOUs)
		{
			Declaration = declaration;
			EndPOUToken = endPOUToken;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return base.BeforeDeclarationStatements;
			yield return Declaration;
			if (_bImplementationBeforeSubPOUs)
			{
				yield return base.Implementation;
			}
			foreach (IWhitePOUSyntax subPOU in base.SubPOUs)
			{
				yield return subPOU;
			}
			if (!_bImplementationBeforeSubPOUs)
			{
				yield return base.Implementation;
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
