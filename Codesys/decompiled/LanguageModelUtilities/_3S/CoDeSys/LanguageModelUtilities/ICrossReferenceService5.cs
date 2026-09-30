using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceService5 : ICrossReferenceService4, ICrossReferenceService3, ICrossReferenceService2, ICrossReferenceService
	{
		ICrossReferenceNode CreateNode(string stName, IAccessInfo accessInfo, IVariable variable, ISignature signature, IExprement expressionAtSourcePosition, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType);
	}
}
