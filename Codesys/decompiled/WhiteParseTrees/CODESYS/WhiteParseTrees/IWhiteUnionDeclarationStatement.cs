using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteUnionDeclarationStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		ITypeToken TypeOp { get; set; }

		IWhiteExpression NameExpression { get; set; }

		IColonToken ColonOp { get; set; }

		IWhiteStatement Declaration { get; set; }

		IEndTypeToken EndTypeOp { get; set; }
	}
}
