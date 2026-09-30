using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteNamespaceDeclarationStatement : IWhitePouDeclarationStatement2, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteDeclarationStatement, IWhitePOU, IWhitePOUSyntax
	{
		[Nullable(1)]
		IEnumerable<IAccessSpecifierToken> Access
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}

		[Nullable(1)]
		IEndNamespaceToken EndNamespace
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
