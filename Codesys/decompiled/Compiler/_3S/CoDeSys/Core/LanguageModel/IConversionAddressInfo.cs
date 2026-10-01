using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConversionAddressInfo : IAddressInfo
	{
		TypeClass From { get; }

		TypeClass To { get; }

		IAddressInfo Base { get; }

		bool Implicit { get; }
	}
}
