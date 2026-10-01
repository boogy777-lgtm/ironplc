using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhitePropertyDeclarationStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IPropertyToken PropertyOp { get; set; }

		IEnumerable<IAccessSpecifierToken> Access { get; set; }

		IWhiteExpression NameExpression { get; set; }

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
