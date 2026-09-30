using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDataManager3 : IDataManager2, IDataManager
	{
		bool IsEmptyArea(int iAreaIndex);
	}
}
