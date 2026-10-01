using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IDirectVariableToken : IWhiteToken, INode
	{
		object Overflow { get; }

		int[] Components { get; }

		DirectVariableSize Size { get; }

		DirectVariableLocation Location { get; }
	}
}
