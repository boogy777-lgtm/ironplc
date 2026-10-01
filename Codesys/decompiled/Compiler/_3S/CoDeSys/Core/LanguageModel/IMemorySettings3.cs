using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IMemorySettings3 : IMemorySettings2, IMemorySettings
	{
		bool BitWordAddressing { get; set; }
	}
}
