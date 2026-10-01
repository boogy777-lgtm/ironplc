using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscJumpTable
	{
		void AddLabel(string stLabel, long lCase);
	}
}
