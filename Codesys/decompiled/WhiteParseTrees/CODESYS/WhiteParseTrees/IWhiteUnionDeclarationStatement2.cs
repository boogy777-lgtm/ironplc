using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteUnionDeclarationStatement2 : IWhiteUnionDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteTypeDeclarationStatement, IWhiteDeclarationStatement
	{
		[Nullable(1)]
		List<IAccessSpecifierToken> Access
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
