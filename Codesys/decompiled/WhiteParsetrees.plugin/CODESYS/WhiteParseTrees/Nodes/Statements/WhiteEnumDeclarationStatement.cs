using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteEnumDeclarationStatement : WhiteStatement, IWhiteEnumDeclarationStatement2, IWhiteEnumDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteTypeDeclarationStatement, IWhiteDeclarationStatement
	{
		public ITypeToken TypeOp { get; set; }

		public List<IAccessSpecifierToken> Access { get; set; }

		public IWhiteExpression NameExpression { get; set; }

		public IEnumerationTypeExpression EnumerationTypeExpression { get; set; }

		public ISemicolonToken Semicolon { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteTypeExpression TypeExpression
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IAssignToken AssignmentOp
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExpression Initialization
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IEndTypeToken EndTypeOp { get; set; }

		public IColonToken ColonOp { get; set; }

		[ExcludeFromCodeCoverage]
		public WhiteEnumDeclarationStatement(ITypeToken typeOp, List<IAccessSpecifierToken> accessSpecifier, IWhiteExpression nameExpression, IColonToken colon, IEnumerationTypeExpression enumerationTypeExpression, [System.Runtime.CompilerServices.Nullable(2)] IWhiteTypeExpression typeExpression, [System.Runtime.CompilerServices.Nullable(2)] IAssignToken assignmentOp, [System.Runtime.CompilerServices.Nullable(2)] IWhiteExpression initialization, ISemicolonToken semicolon, IEndTypeToken endTypeOp)
		{
			TypeOp = typeOp;
			Access = accessSpecifier;
			NameExpression = nameExpression;
			ColonOp = colon;
			EnumerationTypeExpression = enumerationTypeExpression;
			TypeExpression = typeExpression;
			AssignmentOp = assignmentOp;
			Initialization = initialization;
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
			yield return EnumerationTypeExpression;
			if (TypeExpression != null)
			{
				yield return TypeExpression;
			}
			if (AssignmentOp != null)
			{
				yield return AssignmentOp;
			}
			if (Initialization != null)
			{
				yield return Initialization;
			}
			yield return Semicolon;
			yield return EndTypeOp;
		}
	}
}
