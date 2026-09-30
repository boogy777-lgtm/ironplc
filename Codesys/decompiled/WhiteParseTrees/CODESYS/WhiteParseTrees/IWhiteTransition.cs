using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteTransition : IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IWhiteTransitionDeclarationStatement Declaration
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}

		[Nullable(1)]
		IEndTransitionToken EndPOUToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
