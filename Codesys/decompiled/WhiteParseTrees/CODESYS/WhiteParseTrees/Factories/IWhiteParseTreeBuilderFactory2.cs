using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IWhiteParseTreeBuilderFactory2 : IWhiteParseTreeBuilderFactory
	{
		[NullableContext(1)]
		new IInterfaceNameStep2 CreateInterfaceBuilder();
	}
}
