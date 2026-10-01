using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IAdditionalCrossReferenceProvider
	{
		IEnumerable<ICrossReferenceNode> GetAdditionalCrossReferences(string stName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, [Optional] Guid guidApplication);

		IEnumerable<ICrossReferenceNode> GetAdditionalCrossReferences(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, [Optional] Guid guidApplication);
	}
}
