using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteStructDeclarationStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		ITypeToken TypeOp { get; set; }

		IWhiteExpression NameExpression { get; set; }

		[Nullable(2)]
		IExtendsToken ExtendsOp
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IEnumerable<IWhiteExpression> Extends { get; set; }

		IColonToken ColonOp { get; set; }

		IWhiteStatement Declaration { get; set; }

		IEndTypeToken EndTypeOp { get; set; }
	}
}
