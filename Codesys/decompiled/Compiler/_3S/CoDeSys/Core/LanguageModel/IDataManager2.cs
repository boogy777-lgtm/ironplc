using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDataManager2 : IDataManager
	{
		IArea[] Areas { get; }
	}
}
