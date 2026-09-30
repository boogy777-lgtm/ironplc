using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IInterfaceOptionalsStep3 : IInterfaceOptionalsStep2, IStatementBuilder<IWhiteInterfaceDeclarationStatement>, IInterfaceOptionalsStep, IInterfaceExtendsStep
	{
		new IInterfaceOptionalsStep3 Extends(IEnumerable<IWhiteExpression> extends);

		new IInterfaceOptionalsStep3 WithDeclarations(IWhiteSequenceStatement declarations);

		new IInterfaceOptionalsStep3 WithAccess(IEnumerable<IAccessSpecifierToken> access);

		IInterfaceOptionalsStep3 Implements(IEnumerable<IWhiteExpression> implements);
	}
}
