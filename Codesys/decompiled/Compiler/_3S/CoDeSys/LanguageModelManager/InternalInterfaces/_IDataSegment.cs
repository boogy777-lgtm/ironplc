using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IDataSegment : IDataSegment
	{
		bool LogByFlag { get; set; }

		_IMemoryManager MemMan { get; set; }

		bool IsData { get; }

		bool IsConstant { get; }

		bool IsInput { get; }

		bool IsOutput { get; }

		bool IsMemory { get; }

		bool IsRetain { get; }

		bool IsCode { get; }

		new ushort Area { get; set; }

		new int Size { get; set; }

		int DPTableOffset { get; set; }

		IDictionary<DataSegmentFlags, _IMemoryManager> MemMansToFlags { get; set; }

		bool GetFlag(DataSegmentFlags dsFlag);

		void SetFlag(DataSegmentFlags dsFlag, bool bSetTrue);

		bool CheckConsistency();
	}
}
