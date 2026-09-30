using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMemManGap
	{
		int Offset { get; }

		int Size { get; }
	}
}
