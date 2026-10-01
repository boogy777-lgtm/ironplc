using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDeRefAccessInfo3 : IDeRefAccessInfo2, IDeRefAccessInfo, IAddressInfo
	{
		IAddressInfo4 GetSizeAdjustedBase();
	}
}
