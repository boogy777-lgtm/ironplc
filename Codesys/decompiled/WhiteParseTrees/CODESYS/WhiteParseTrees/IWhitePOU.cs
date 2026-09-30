using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhitePOU : IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IWhiteSequenceStatement BeforeDeclarationStatements { get; }

		IWhiteDeclarationStatement DeclarationStatement { get; }

		IWhiteSequenceStatement Implementation { get; }

		IEnumerable<IWhitePOUSyntax> SubPOUs { get; }

		IEndPouTypeToken EndPOU { get; }
	}
}
