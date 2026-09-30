using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ISimpleFilteredAdditionalCrossReferenceProvider
	{
		IEnumerable<ICrossReferenceNode> ProvideAdditionalCrossReferences(Predicate<string> prefiltering);
	}
}
