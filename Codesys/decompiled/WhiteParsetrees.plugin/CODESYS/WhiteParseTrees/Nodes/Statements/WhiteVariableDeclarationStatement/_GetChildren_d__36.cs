using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteVariableDeclarationStatement : WhiteStatement, IWhiteVariableDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		public IEnumerable<IWhiteExpression> VariableNames { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IAtToken At
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteAnyDirectVariableExpression AddressLocation
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public IColonToken Colon { get; set; }

		public IWhiteTypeExpression DeclaredType { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IAnyAssignmentToken Assignment
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteExpression InitializationExpression
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public ISemicolonToken Semicolon { get; set; }

		public WhiteVariableDeclarationStatement(IList<IWhiteExpression> variableNames, [System.Runtime.CompilerServices.Nullable(2)] IAtToken at, [System.Runtime.CompilerServices.Nullable(2)] IWhiteAnyDirectVariableExpression addressLocation, IColonToken colon, IWhiteTypeExpression declaredType, [System.Runtime.CompilerServices.Nullable(2)] IAnyAssignmentToken assignment, [System.Runtime.CompilerServices.Nullable(2)] IWhiteExpression initializationExpression, ISemicolonToken semicolon)
		{
			VariableNames = variableNames;
			At = at;
			AddressLocation = addressLocation;
			Colon = colon;
			DeclaredType = declaredType;
			Assignment = assignment;
			InitializationExpression = initializationExpression;
			Semicolon = semicolon;
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
			foreach (IWhiteExpression variableName in VariableNames)
			{
				yield return variableName;
			}
			if (At != null)
			{
				yield return At;
			}
			if (AddressLocation != null)
			{
				yield return AddressLocation;
			}
			yield return Colon;
			yield return DeclaredType;
			if (Assignment != null)
			{
				yield return Assignment;
			}
			if (InitializationExpression != null)
			{
				yield return InitializationExpression;
			}
			yield return Semicolon;
		}
	}
}
