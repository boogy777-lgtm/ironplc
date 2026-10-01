using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMemoryAllocationCallbackEmbedded
	{
		int GlobalCodeReserve(ICompileContext12 comcon);
	}
}
