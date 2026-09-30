using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IWhiteParseTreeStatementFactory2 : IWhiteParseTreeStatementFactory
	{
		[NullableContext(1)]
		IWhiteInterfaceDeclarationStatement CreateWhiteInterfaceDeclarationStatement(IInterfaceToken pouClass, IEnumerable<IAccessSpecifierToken> access, IWhiteExpression nameExpression, [Nullable(2)] IExtendsToken extendsOp, IEnumerable<IWhiteExpression> extends, IWhiteSequenceStatement declarations);
	}
}
