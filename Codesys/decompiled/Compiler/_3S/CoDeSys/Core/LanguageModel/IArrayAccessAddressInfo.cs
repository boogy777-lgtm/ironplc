using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArrayAccessAddressInfo : IAddressInfo
	{
		IAddressInfo Base { get; }

		IAddressInfo[] Indexes { get; }

		IArrayBounds[] Bounds { get; }
	}
}
