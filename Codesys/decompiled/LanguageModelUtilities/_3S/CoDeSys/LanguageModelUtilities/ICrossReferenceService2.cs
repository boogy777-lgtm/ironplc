using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceService2 : ICrossReferenceService
	{
		ICrossReferenceNode CreateNode(string stName, IAccessInfo accessInfo, IVariable variable, ISignature signature, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType);

		IIdentifierInfo2 CreateIdentifierInfo(ISignature containingSignature, string stName, string stComment, IdentifierInfoFlag flags, IType type, IVariable variable, ISignature signature, IScope scope);
	}
}
