using System.Collections.Generic;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class CrossRefUtils
	{
		internal static IEnumerable<ICrossReferenceNode> RetrieveAdditionalCrossRefs(ISimpleFilteredAdditionalCrossReferenceProvider provider, string prefilterString)
		{
			string searchStringLowerCase = prefilterString.ToLowerInvariant();
			return provider.ProvideAdditionalCrossReferences((string s) => s.ToLowerInvariant().Contains(searchStringLowerCase));
		}
	}
}
