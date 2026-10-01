using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVFTableEntry2 : IVFTableEntry
	{
		bool IsFunctionPointerEntry { get; }

		int Id { get; }
	}
}
