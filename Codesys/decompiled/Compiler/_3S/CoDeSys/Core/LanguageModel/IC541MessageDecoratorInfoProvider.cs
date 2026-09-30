using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IC541MessageDecoratorInfoProvider
	{
		IEnumerable<Guid> PlugInGuids { get; }

		string GetProvidingAddOnName(Guid gdMemoryAllocationCallback);
	}
}
