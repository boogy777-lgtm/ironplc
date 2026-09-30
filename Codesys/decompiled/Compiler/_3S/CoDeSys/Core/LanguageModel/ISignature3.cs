using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISignature3 : ISignature2, ISignature
	{
		uint Checksum { get; }

		uint ChecksumNoInit { get; }

		string Comment { get; }
	}
}
