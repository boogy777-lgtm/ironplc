using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteFunctionBlockDeclarationStatement : WhiteBaseDeclarationStatement, IWhiteFunctionBlockDeclarationStatement, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhitePouDeclarationStatement2, IWhiteDeclarationStatement
	{
		public IEnumerable<IAccessSpecifierToken> Access { get; set; }

		public IWhiteSequenceStatement GenericDeclarations { get; set; }

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

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IImplementsToken ImplementsOp
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IEnumerable<IWhiteExpression> Implements { get; set; }

		public WhiteFunctionBlockDeclarationStatement(IPouTypeToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IWhiteSequenceStatement genericDeclarations, [System.Runtime.CompilerServices.Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, [System.Runtime.CompilerServices.Nullable(2)] IImplementsToken implementsOp, IEnumerable<IWhiteExpression> implements, IWhiteSequenceStatement declarations)
			: base(pouClass, nameExpression, declarations)
		{
			Access = access;
			GenericDeclarations = genericDeclarations;
			ExtendsOp = extendsOp;
			Extends = extends;
			ImplementsOp = implementsOp;
			Implements = implements;
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
			yield return base.Class;
			foreach (IAccessSpecifierToken item in Access)
			{
				yield return item;
			}
			yield return base.NameExpression;
			yield return GenericDeclarations;
			if (ExtendsOp != null)
			{
				yield return ExtendsOp;
			}
			foreach (IWhiteExpression extend in Extends)
			{
				yield return extend;
			}
			if (ImplementsOp != null)
			{
				yield return ImplementsOp;
			}
			foreach (IWhiteExpression implement in Implements)
			{
				yield return implement;
			}
			yield return base.Declarations;
		}
	}
}
