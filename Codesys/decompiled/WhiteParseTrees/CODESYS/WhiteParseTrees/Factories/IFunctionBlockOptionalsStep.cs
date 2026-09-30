using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IFunctionBlockOptionalsStep : IFunctionBlockImplementsStep, IFunctionBlockExtendsStep, IFunctionBlockAccessStep
	{
		[NullableContext(1)]
		IFunctionBlockBuilder WithDeclarations(IWhiteSequenceStatement declarations);

		[NullableContext(1)]
		IFunctionBlockOptionalsStep WithGenericDeclarations(IWhiteSequenceStatement declarations);
	}
}
