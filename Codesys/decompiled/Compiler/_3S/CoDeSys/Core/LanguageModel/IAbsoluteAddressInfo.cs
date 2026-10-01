using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAbsoluteAddressInfo : IAddressInfo
	{
		int Area { get; }

		int Offset { get; }

		byte BitOffset { get; }
	}
}
