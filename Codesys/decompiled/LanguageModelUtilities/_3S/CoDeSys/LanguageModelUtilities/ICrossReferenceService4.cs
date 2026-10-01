using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceService4 : ICrossReferenceService3, ICrossReferenceService2, ICrossReferenceService
	{
		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinApplication(ISignature signature, IVariable variable, string stNewName, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidApplication);

		IList<ICrossReferenceNode> GetCrossReferenceForVariable(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidSignature);
	}
}
