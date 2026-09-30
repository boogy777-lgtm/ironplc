using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompilerVersionManager2 : ICompilerVersionManager
	{
		bool UseNewestVersion { get; set; }

		event UseNewestCompilerVersionChangingEventHandler UseNewestCompilerVersionChanging;

		event UseNewestCompilerVersionChangedEventHandler UseNewestCompilerVersionChanged;

		event CompilerVersionChangingEventHandler CompilerVersionChanging;

		event CompilerVersionChangedEventHandler CompilerVersionChanged;

		void SetCompilerVersion(Version version);
	}
}
