using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceService3 : ICrossReferenceService2, ICrossReferenceService
	{
		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinProjectAndSourceLibraries(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType);

		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinProjectAndSourceLibraries(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType);
	}
}
