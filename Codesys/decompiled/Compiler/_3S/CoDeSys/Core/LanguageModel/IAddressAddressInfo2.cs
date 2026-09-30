using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressAddressInfo2 : IAddressAddressInfo, IAddressInfo
	{
		int Area { get; }

		int Offset { get; }

		void ConvertToAbsoluteAddress(ulong ulAreaStartAddress);
	}
}
