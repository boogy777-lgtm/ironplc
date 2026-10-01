using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDataSegment
	{
		DataSegmentFlags Flags { get; }

		int SizeAllocated { get; }

		int MaxContiguosMemory { get; }

		ushort Area { get; }

		int Address { get; }

		int Size { get; }

		IEnumerable<IMemManGap> Gaps { get; }
	}
}
