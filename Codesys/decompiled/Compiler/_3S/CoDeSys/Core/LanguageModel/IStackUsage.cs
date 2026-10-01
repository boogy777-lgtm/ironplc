using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IStackUsage
	{
		IEnumerable<IStackUsageEntry> Entries { get; }

		int MaxStackSize { get; }

		int MaxStackSizeForExternalCalls { get; }

		IScope Scope { get; }

		bool StackOverflow { get; }
	}
}
