using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhitePropertyAccessorDeclarationStatement : IWhitePouDeclarationStatement2, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhiteDeclarationStatement
	{
		[Nullable(1)]
		IEnumerable<IAccessSpecifierToken> Access
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}

		[Nullable(2)]
		IColonToken Colon
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(2)]
		IWhiteTypeExpression ReturnType
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}
	}
}
