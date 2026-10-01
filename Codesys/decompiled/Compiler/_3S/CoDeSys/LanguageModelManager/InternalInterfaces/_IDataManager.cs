using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IDataManager : IDataManager4, IDataManager3, IDataManager2, IDataManager
	{
		int Count { get; }

		int DataSegmentSize { get; }

		int PackMode { get; }

		int MinSize { get; }

		int StackAlignment { get; }

		int CodeSegmentSize { get; }

		bool RetainInOwnSegment { get; }

		bool ByteAddressing { get; }

		bool BitByteAddressing { get; }

		bool BitWordAddressing { get; }

		new int FirstArea { get; set; }

		_IDataSegment this[DataSegmentFlags flags] { get; }

		_IDataSegment this[ushort usRefId] { get; }

		_IMemorySettings _MemorySettings { get; set; }

		_IDataManager Reference { get; set; }

		_IDataSegment this[int i] { get; }

		_IDataSegment[] AreaSegments { get; }

		bool HasConstantSegment { get; }

		IList<_IDataSegment> _DataSegments { get; }

		IList<_IDataSegment> _AreaSegments { get; }

		IList<IArea> _Areas { get; }

		_IArea GetArea(int i);

		_IArea GetAreaByIndex(int nAreaIndex);

		void RemoveArea(_IArea area);

		void AddArea(_IArea area);

		bool IsPersistentSupported();

		bool IsRetainSupported();

		_IDataSegment GetPreferredDataSegment(DataSegmentFlags flags);

		_IDataSegment[] AllDataSegmentsByFlag(DataSegmentFlags flags);

		_IDataSegment GetAreaSegment(int i);

		bool CheckConsistency();
	}
}
