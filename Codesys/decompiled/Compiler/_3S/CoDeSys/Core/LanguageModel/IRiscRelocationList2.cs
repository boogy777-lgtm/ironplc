using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscRelocationList2 : IRiscRelocationList, IRelocationList
	{
		void AddCallRelocation(int nCurrentOffset, int nSignatureId);
	}
}
