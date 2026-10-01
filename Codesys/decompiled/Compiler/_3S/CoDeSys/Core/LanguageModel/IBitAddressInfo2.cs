using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IBitAddressInfo2 : IBitAddressInfo, IAddressInfo
	{
		int AccessedElementSize { get; }
	}
}
