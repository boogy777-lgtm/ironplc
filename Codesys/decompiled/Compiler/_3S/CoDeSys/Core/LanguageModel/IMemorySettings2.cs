using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMemorySettings2 : IMemorySettings
	{
		IArea CreateArea(DataSegmentFlags dastFlags, KindOfArea kindof, int nMinimalSize, int nStartAddress, int nAllocationPlusinPercent, int nMaximalAreaSize);
	}
}
