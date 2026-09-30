using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IMemorySettings : IMemorySettings3, IMemorySettings2, IMemorySettings
	{
		bool OnlineChangeInOwnSegment { get; set; }

		bool AdditionalAreas { get; set; }

		bool OneSRAM { get; set; }

		bool RetainDynamic { get; set; }

		bool PersistentDynamic { get; set; }

		int CodeSegmentHeaderSize { get; set; }

		int MaxSizeForOnlineChange { get; set; }

		int MinGranularity { get; set; }

		_IArea[] _Areas { get; }

		int StaticAreaSize { get; set; }

		int AreaAlignment { get; set; }

		IDictionary<Guid, IList<_IDataSegment>> MappedDataSegmentsPerApp { get; }

		IList<_IDataSegment> MappedDataSegments { get; }

		uint CalculateChecksum();
	}
}
