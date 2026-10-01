using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteDUT : IWhitePOU, IWhitePOUSyntax, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IWhiteTypeDeclarationStatement TypeDeclarationStatement
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
