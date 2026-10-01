using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IArea : IArea4, IArea3, IArea2, IArea
	{
		bool Dynamic { get; set; }

		new uint Checksum { get; set; }

		AreaFlags AreaFlags { get; }

		DataSegmentFlags DataSegmentFlags { get; }

		new int MinimalAreaSize { get; set; }

		new int MaximalAreaSize { get; set; }

		int AllocationPlusInPercent { get; set; }

		new int Index { get; set; }

		new int Size { get; set; }

		int AvailableSize { get; set; }

		new int StartAddress { get; set; }

		new bool Automatic { get; set; }

		new bool OnlineChangeArea { get; }

		bool GetAreaFlag(AreaFlags aflag);

		void SetAreaFlag(AreaFlags aflag, bool bSetTrue);

		bool GetDataSegmentFlag(DataSegmentFlags dsFlag);

		void SetDataSegmentFlag(DataSegmentFlags dsFlag, bool bSetTrue);

		_IArea Duplicate();
	}
}
