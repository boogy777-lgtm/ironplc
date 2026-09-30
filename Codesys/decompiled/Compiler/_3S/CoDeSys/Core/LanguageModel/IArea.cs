using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArea
	{
		int Index { get; }

		int Size { get; }

		int StartAddress { get; }

		bool Automatic { get; }

		DataSegmentFlags Flags { get; }
	}
}
