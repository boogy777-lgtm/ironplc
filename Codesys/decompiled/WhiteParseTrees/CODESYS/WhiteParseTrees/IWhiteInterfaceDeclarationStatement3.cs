using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteInterfaceDeclarationStatement3 : IWhiteInterfaceDeclarationStatement2, IWhiteInterfaceDeclarationStatement, IWhitePouDeclarationStatement, IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IWhitePouDeclarationStatement2, IWhiteDeclarationStatement
	{
		[Nullable(2)]
		IImplementsToken ImplementsOp
		{
			[NullableContext(2)]
			get;
			[NullableContext(2)]
			set;
		}

		[Nullable(1)]
		IEnumerable<IWhiteExpression> Implements
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
