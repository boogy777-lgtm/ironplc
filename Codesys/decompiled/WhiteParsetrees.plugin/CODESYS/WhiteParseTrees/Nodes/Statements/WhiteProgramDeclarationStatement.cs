using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	public class WhiteProgramDeclarationStatement : WhiteBaseDeclarationStatement, IWhiteProgramDeclarationStatement, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhitePouDeclarationStatement2, IWhiteDeclarationStatement
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

		[System.Runtime.CompilerServices.NullableContext(1)]
		public WhiteProgramDeclarationStatement(IPouTypeToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IWhiteSequenceStatement declarations)
			: base(pouClass, nameExpression, declarations)
		{
			Access = access;
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
			yield return base.Declarations;
		}
	}
}
