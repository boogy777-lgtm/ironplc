using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public class CompilerVersionChangedEventArgs : CompilerVersionEventArgsBase
	{
		public CompilerVersionChangedEventArgs(Version previousVersion, Version newVersion)
			: base(previousVersion, newVersion)
		{
		}
	}
}
