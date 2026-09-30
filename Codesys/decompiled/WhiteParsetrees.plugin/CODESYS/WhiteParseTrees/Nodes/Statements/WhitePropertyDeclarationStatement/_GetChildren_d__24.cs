using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhitePropertyDeclarationStatement : WhiteStatement, IWhitePropertyDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteDeclarationStatement
	{
		public IPropertyToken PropertyOp { get; set; }

		public IEnumerable<IAccessSpecifierToken> Access { get; set; }

		public IWhiteExpression NameExpression { get; set; }

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

		public WhitePropertyDeclarationStatement(IPropertyToken propertyOp, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, IColonToken colon, IWhiteTypeExpression returnType)
		{
			PropertyOp = propertyOp;
			Access = access;
			NameExpression = nameExpression;
			Colon = colon;
			ReturnType = returnType;
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
			yield return PropertyOp;
			foreach (IAccessSpecifierToken item in Access)
			{
				yield return item;
			}
			yield return NameExpression;
			if (Colon != null)
			{
				yield return Colon;
			}
			if (ReturnType != null)
			{
				yield return ReturnType;
			}
		}
	}
}
