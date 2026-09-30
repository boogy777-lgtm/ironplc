using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	public class WhiteMethodDeclarationStatement : WhiteBaseDeclarationStatement, IWhiteMethodDeclarationStatement, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhitePouDeclarationStatement2, IWhiteDeclarationStatement
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		public IEnumerable<IAccessSpecifierToken> Access
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
			[System.Runtime.CompilerServices.NullableContext(1)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IColonToken Colon
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteTypeExpression ReturnType
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public WhiteMethodDeclarationStatement(IPouTypeToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IColonToken colon, [System.Runtime.CompilerServices.Nullable(2)] IWhiteTypeExpression returnType, IWhiteSequenceStatement declarations)
			: base(pouClass, nameExpression, declarations)
		{
			Access = access;
			Colon = colon;
			ReturnType = returnType;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override void Accept(IStatementSyntax.IStatementVisitor visitor)
		{
			visitor.visit(this);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IStatementSyntax.IStatementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IStatementSyntax.IStatementVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public override IEnumerable<INode> GetChildren()
		{
			yield return base.Class;
			foreach (IAccessSpecifierToken item in Access)
			{
				yield return item;
			}
			yield return base.NameExpression;
			if (Colon != null)
			{
				yield return Colon;
			}
			if (ReturnType != null)
			{
				yield return ReturnType;
			}
			yield return base.Declarations;
		}
	}
}
