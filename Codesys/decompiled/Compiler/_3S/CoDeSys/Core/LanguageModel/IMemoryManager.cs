using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMemoryManager
	{
		int Size { get; }

		int SizeAllocated { get; }
	}
}
