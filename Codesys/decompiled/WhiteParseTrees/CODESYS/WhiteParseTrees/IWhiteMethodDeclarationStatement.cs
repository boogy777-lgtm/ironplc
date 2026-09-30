using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(2)]
	[ReleasedInterface]
	public interface IWhiteMethodDeclarationStatement : IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(1)]
		IEnumerable<IAccessSpecifierToken> Access
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}

		IColonToken Colon { get; set; }

		IWhiteTypeExpression ReturnType { get; set; }
	}
}
