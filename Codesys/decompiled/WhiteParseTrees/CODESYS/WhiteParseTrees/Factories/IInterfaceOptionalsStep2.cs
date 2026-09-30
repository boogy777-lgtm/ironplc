using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IInterfaceOptionalsStep2 : IStatementBuilder<IWhiteInterfaceDeclarationStatement>, IInterfaceOptionalsStep, IInterfaceExtendsStep
	{
		new IInterfaceOptionalsStep2 Extends(IEnumerable<IWhiteExpression> extends);

		new IInterfaceOptionalsStep2 WithDeclarations(IWhiteSequenceStatement declarations);

		IInterfaceOptionalsStep2 WithAccess(IEnumerable<IAccessSpecifierToken> access);
	}
}
