using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPropertyAddressInfo : IAddressInfo
	{
		int AreaProperty { get; }

		int OffsetGet { get; }

		int OffsetSet { get; }

		int AreaInstance { get; }

		int OffsetInstance { get; }
	}
}
