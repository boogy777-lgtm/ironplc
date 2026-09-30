using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPropertyAddressInfoExtended : IAddressInfo
	{
		IAddressInfo InfoInstance { get; }

		bool InterfaceCall { get; }

		int VFTableOffsetGet { get; }

		int VFTableOffsetSet { get; }
	}
}
