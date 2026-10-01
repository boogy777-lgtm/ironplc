using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IInterfaceOptionalsStep : IInterfaceExtendsStep
	{
		[NullableContext(1)]
		IInterfaceBuilder WithDeclarations(IWhiteSequenceStatement declarations);
	}
}
