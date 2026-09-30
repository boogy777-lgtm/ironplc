using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees.Factories
{
	[ReleasedInterface]
	public interface IInterfaceNameStep2 : IInterfaceNameStep
	{
		[NullableContext(1)]
		new IInterfaceOptionalsStep2 WithName(IWhiteExpression nameExpression);
	}
}
