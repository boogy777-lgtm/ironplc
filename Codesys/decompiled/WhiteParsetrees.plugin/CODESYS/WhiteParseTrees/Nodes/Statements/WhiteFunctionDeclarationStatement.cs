using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(2)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteFunctionDeclarationStatement : WhiteBaseDeclarationStatement, IWhiteFunctionDeclarationStatement, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhitePouDeclarationStatement2, IWhiteDeclarationStatement
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

		public IColonToken Colon { get; set; }

		public IWhiteTypeExpression ReturnType { get; set; }

		public ISemicolonToken ReturnTypeTrailingSemicolon { get; set; }

		[System.Runtime.CompilerServices.NullableContext(1)]
		public WhiteFunctionDeclarationStatement(IPouTypeToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [System.Runtime.CompilerServices.Nullable(2)] IColonToken colon, [System.Runtime.CompilerServices.Nullable(2)] IWhiteTypeExpression returnType, [System.Runtime.CompilerServices.Nullable(2)] ISemicolonToken returnTypeTrailingSemicolon, IWhiteSequenceStatement declarations)
			: base(pouClass, nameExpression, declarations)
		{
			Access = access;
			Colon = colon;
			ReturnType = returnType;
			ReturnTypeTrailingSemicolon = returnTypeTrailingSemicolon;
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
			if (ReturnTypeTrailingSemicolon != null)
			{
				yield return ReturnTypeTrailingSemicolon;
			}
			yield return base.Declarations;
		}
	}
}
