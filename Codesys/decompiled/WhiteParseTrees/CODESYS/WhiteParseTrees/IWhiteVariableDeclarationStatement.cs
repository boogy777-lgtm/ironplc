using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteVariableDeclarationStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IEnumerable<IWhiteExpression> VariableNames { get; set; }

		[Nullable(2)]
		IAtToken At
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IWhiteAnyDirectVariableExpression AddressLocation
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IColonToken Colon { get; set; }

		IWhiteTypeExpression DeclaredType { get; set; }

		[Nullable(2)]
		IAnyAssignmentToken Assignment
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IWhiteExpression InitializationExpression
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		ISemicolonToken Semicolon { get; set; }
	}
}
