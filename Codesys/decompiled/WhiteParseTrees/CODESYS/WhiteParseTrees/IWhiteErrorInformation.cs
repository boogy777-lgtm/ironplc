using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteErrorInformation
	{
		IWhiteErrorStatement ErrorStatement { get; set; }

		int CharacterOffset { get; set; }
	}
}
