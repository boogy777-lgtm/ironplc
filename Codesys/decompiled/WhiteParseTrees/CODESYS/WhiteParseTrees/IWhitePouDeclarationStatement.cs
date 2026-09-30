using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhitePouDeclarationStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IPouTypeToken Class { get; set; }

		IWhiteExpression NameExpression { get; set; }

		IWhiteSequenceStatement Declarations { get; set; }
	}
}
