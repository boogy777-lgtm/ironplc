using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMemorySettings
	{
		int PackMode { get; set; }

		int MinSize { get; set; }

		int StackAlignment { get; set; }

		bool ByteAddressing { get; set; }

		bool BitByteAddressing { get; set; }

		bool RetainInOwnSegment { get; set; }

		int DataSegmentSize { get; set; }

		int CodeSegmentSize { get; set; }

		int CodeSegmentPrologSize { get; set; }

		int GlobalDataSize { get; set; }

		int CodeSize { get; set; }

		int MemoryDataSize { get; set; }

		int InputDataSize { get; set; }

		int OutputDataSize { get; set; }

		int RetainDataSize { get; set; }

		int MaxDataSize { get; set; }

		int MaxCodeSize { get; set; }

		IArea[] Areas { get; }

		void AddArea(IArea area);

		IArea CreateArea(DataSegmentFlags dastFlags, KindOfArea kindof, int nMinimalSize, int nStartAddress, int nAllocationPlusinPercent);
	}
}
