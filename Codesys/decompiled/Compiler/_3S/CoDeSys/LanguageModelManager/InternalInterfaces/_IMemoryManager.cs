using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IMemoryManager : IMemoryManager
	{
		int SizeWithoutGaps { get; }

		int MaxContiguosMemory { get; }

		int Count { get; }

		_IMemManGap this[int i] { get; }

		bool AllocateHard(int iOffset, int iSize);

		bool Allocate(int iOffset, int iSize);

		bool CheckConsistency();

		_IMemoryManager Duplicate();

		bool Free(int iOffset, int iSize);

		int AllocateHighestAddress(int iGranularity, int iSegmentSize, int iSizeRequiredPrm, int iPackMode, int iMinSize, out bool bSuccess);

		int AllocateWithoutGap(int iGranularity, int iSegmentSize, int iSizeRequiredPrm, int iPackMode, int iMinSize, out bool bSuccess);

		int Allocate(int iGranularity, int iSegmentSize, int iSizeRequiredPrm, int iPackMode, int iMinSize, out bool bSuccess);

		bool Shrink(int nNewSize);

		bool Shrink(int nMinSize, int nAdditionalPercentage, int nMaxSize);

		bool Shrink(int nMinSize, int nAdditionalPercentage, int nMaxSize, int areaAlign);

		void DeleteGapsButLast();
	}
}
