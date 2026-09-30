using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRelocationAreaList
	{
		int Area { get; }

		IRelocation[] Relocations { get; }
	}
}
