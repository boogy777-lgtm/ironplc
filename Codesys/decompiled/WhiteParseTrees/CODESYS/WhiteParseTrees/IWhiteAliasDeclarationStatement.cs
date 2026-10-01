using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteAliasDeclarationStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		ITypeToken TypeOp { get; set; }

		IWhiteExpression NameExpression { get; set; }

		IColonToken ColonOp { get; set; }

		IWhiteTypeExpression Type { get; set; }

		ISemicolonToken Semicolon { get; set; }

		IEndTypeToken EndTypeOp { get; set; }
	}
}
