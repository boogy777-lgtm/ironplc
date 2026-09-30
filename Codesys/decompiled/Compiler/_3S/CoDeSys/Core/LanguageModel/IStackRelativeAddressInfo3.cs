using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStackRelativeAddressInfo3 : IStackRelativeAddressInfo2, IStackRelativeAddressInfo, IAddressInfo
	{
		ulong BasePointer { get; set; }
	}
}
