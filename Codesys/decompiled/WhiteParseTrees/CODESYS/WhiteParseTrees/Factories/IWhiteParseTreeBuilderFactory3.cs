using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IWhiteParseTreeBuilderFactory3 : IWhiteParseTreeBuilderFactory2, IWhiteParseTreeBuilderFactory
	{
		[NullableContext(1)]
		new IInterfaceNameStep3 CreateInterfaceBuilder();
	}
}
