using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledPOU3 : ICompiledPOU
	{
		uint Checksum { get; }
	}
}
