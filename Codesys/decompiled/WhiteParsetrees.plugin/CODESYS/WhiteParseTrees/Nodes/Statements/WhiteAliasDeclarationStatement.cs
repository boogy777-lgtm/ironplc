using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteAliasDeclarationStatement : WhiteStatement, IWhiteAliasDeclarationStatement2, IWhiteAliasDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteTypeDeclarationStatement, IWhiteDeclarationStatement
	{
		public ITypeToken TypeOp { get; set; }

		public List<IAccessSpecifierToken> Access { get; set; }

		public IWhiteExpression NameExpression { get; set; }

		public IWhiteTypeExpression Type { get; set; }

		public ISemicolonToken Semicolon { get; set; }

		public IEndTypeToken EndTypeOp { get; set; }

		public IColonToken ColonOp { get; set; }

		public WhiteAliasDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> accessSpecifier, IWhiteExpression nameExpression, IColonToken colon, IWhiteTypeExpression type, ISemicolonToken semicolon, IEndTypeToken endTypeOp)
		{
			TypeOp = typeOp;
			Access = accessSpecifier;
			NameExpression = nameExpression;
			ColonOp = colon;
			Type = type;
			Semicolon = semicolon;
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
			yield return ColonOp;
			yield return Type;
			yield return Semicolon;
			yield return EndTypeOp;
		}
	}
}
