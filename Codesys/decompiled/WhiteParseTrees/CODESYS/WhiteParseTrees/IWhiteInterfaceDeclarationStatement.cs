using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteInterfaceDeclarationStatement : IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax
	{
		[Nullable(2)]
		IExtendsToken ExtendsOp
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(1)]
		IEnumerable<IWhiteExpression> Extends
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
