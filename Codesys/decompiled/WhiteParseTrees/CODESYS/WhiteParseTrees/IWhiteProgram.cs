using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteProgram : IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IWhiteProgramDeclarationStatement Declaration
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}

		[Nullable(1)]
		IEndProgramToken2 EndPOUToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
