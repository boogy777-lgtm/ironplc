using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class UseNewestCompilerVersionChangedEventArgs : UseNewestCompilerVersionEventArgsBase
	{
		public UseNewestCompilerVersionChangedEventArgs(bool bOldValue, bool bNewValue)
			: base(bOldValue, bNewValue)
		{
		}
	}
}
