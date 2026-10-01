using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteFunctionBlockDeclarationStatement : IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		IEnumerable<IAccessSpecifierToken> Access { get; set; }

		IWhiteSequenceStatement GenericDeclarations { get; set; }

		[Nullable(2)]
		IExtendsToken ExtendsOp
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IEnumerable<IWhiteExpression> Extends { get; set; }

		[Nullable(2)]
		IImplementsToken ImplementsOp
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		IEnumerable<IWhiteExpression> Implements { get; set; }
	}
}
