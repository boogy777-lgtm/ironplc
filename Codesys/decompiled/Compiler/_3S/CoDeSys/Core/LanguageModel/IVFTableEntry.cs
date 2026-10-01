using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVFTableEntry
	{
		string Name { get; }

		bool IsEqual(IVFTableEntry vftableIn);

		IVFTableEntry Duplicate();
	}
}
