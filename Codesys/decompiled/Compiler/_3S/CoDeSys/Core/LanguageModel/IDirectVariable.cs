using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDirectVariable
	{
		DirectVariableLocation Location { get; }

		DirectVariableSize Size { get; }

		int[] Components { get; }

		bool Incomplete { get; }

		bool IsEqual(IDirectVariable rhs);
	}
}
