using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteVariableDeclarationListStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IVarListStartToken BeginVarOp { get; set; }

		IEnumerable<IWhiteOperatorToken> PersistantRetain { get; set; }

		IWhiteSequenceStatement Declarations { get; set; }

		IVarListEndToken EndVarOp { get; set; }
	}
}
