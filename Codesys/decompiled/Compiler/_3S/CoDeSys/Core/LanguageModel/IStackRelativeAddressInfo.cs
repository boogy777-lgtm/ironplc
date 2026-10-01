using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStackRelativeAddressInfo : IAddressInfo
	{
		int Offset { get; }

		byte BitOffset { get; }

		int AreaCode { get; }

		int OffsetCode { get; }

		int SizeCode { get; }
	}
}
