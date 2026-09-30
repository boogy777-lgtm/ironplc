using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteTransitionDeclarationStatement : WhiteBaseDeclarationStatement, IWhiteTransitionDeclarationStatement, IWhitePouDeclarationStatement2, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteDeclarationStatement
	{
		public IEnumerable<IErrorToken> Access { get; set; }

		public IEnumerable<IErrorToken> ExtendsOrImplements { get; set; }

		internal WhiteTransitionDeclarationStatement(IPouTypeToken pouClass, IEnumerable<IErrorToken> access, IWhiteExpression nameExpression, IEnumerable<IErrorToken> extendsOrImplements, IWhiteSequenceStatement declarations)
			: base(pouClass, nameExpression, declarations)
		{
			Access = access;
			ExtendsOrImplements = extendsOrImplements;
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
			yield return base.Class;
			foreach (IErrorToken item in Access)
			{
				yield return item;
			}
			yield return base.NameExpression;
			foreach (IErrorToken extendsOrImplement in ExtendsOrImplements)
			{
				yield return extendsOrImplement;
			}
			yield return base.Declarations;
		}
	}
}
