using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressAddressInfo : IAddressInfo
	{
		ulong Address { get; }
	}
}
