using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExternalReference2 : IExternalReference
	{
		ISignature2 Signature { get; }

		uint CRC { get; }
	}
}
