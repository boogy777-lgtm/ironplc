using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressInfo3 : IAddressInfo2, IAddressInfo
	{
		IType Type { get; }
	}
}
