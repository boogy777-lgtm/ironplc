using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompilePrecondition
	{
		bool IsDone { get; }

		event EventHandler WhenDone;
	}
}
