using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscRelocationList : IRelocationList
	{
		void AddRelocation(int iArea, int nCurrentOffset);
	}
}
