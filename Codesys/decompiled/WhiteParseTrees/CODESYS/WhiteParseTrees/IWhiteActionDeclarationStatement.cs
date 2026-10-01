using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteActionDeclarationStatement : IWhitePouDeclarationStatement2, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteDeclarationStatement
	{
		[Nullable(1)]
		IEnumerable<IErrorToken> Access
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}

		[Nullable(1)]
		IEnumerable<IErrorToken> ExtendsOrImplements
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
