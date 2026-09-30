using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteStructDeclarationStatement : WhiteStatement, IWhiteStructDeclarationStatement2, IWhiteStructDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteTypeDeclarationStatement, IWhiteDeclarationStatement
	{
		public ITypeToken TypeOp { get; set; }

		public List<IAccessSpecifierToken> Access { get; set; }

		public IWhiteExpression NameExpression { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IExtendsToken ExtendsOp
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IEnumerable<IWhiteExpression> Extends { get; set; }

		public IWhiteStatement Declaration { get; set; }

		public IEndTypeToken EndTypeOp { get; set; }

		public IColonToken ColonOp { get; set; }

		public WhiteStructDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> accessSpecifier, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, IColonToken colon, IWhiteStatement declaration, IEndTypeToken endTypeOp)
		{
			TypeOp = typeOp;
			Access = accessSpecifier;
			NameExpression = nameExpression;
			ExtendsOp = extendsOp;
			Extends = extends;
			ColonOp = colon;
			Declaration = declaration;
			EndTypeOp = endTypeOp;
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
			yield return TypeOp;
			foreach (IAccessSpecifierToken item in Access)
			{
				yield return item;
			}
			yield return NameExpression;
			if (ExtendsOp != null)
			{
				yield return ExtendsOp;
			}
			foreach (IWhiteExpression extend in Extends)
			{
				yield return extend;
			}
			yield return ColonOp;
			yield return Declaration;
			yield return EndTypeOp;
		}
	}
}
