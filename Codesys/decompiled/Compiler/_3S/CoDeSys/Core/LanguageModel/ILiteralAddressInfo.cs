using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILiteralAddressInfo : IAddressInfo
	{
		ILiteralValue Value { get; }

		IType Type { get; }
	}
}
