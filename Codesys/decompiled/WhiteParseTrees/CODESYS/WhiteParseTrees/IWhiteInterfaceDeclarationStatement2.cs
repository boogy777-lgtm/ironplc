using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteInterfaceDeclarationStatement2 : IWhiteInterfaceDeclarationStatement, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IEnumerable<IAccessSpecifierToken> Access
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
