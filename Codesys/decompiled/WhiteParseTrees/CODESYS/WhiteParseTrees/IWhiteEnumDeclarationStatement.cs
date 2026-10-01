using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteEnumDeclarationStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		ITypeToken TypeOp { get; set; }

		IWhiteExpression NameExpression { get; set; }

		IColonToken ColonOp { get; set; }

		IEnumerationTypeExpression EnumerationTypeExpression { get; set; }

		[Nullable(2)]
		IWhiteTypeExpression TypeExpression
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IAssignToken AssignmentOp
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IWhiteExpression Initialization
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		ISemicolonToken Semicolon { get; set; }

		IEndTypeToken EndTypeOp { get; set; }
	}
}
