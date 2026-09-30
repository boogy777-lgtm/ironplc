using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceService
	{
		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinProject(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType);

		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinApplication(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidApplication);

		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinSignature(string stOldName, string stNewName, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidSignature);

		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinProject(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType);

		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinApplication(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidApplication);

		IList<ICrossReferenceNode> GetCrossReferenceNodesWithinSignature(Regex regex, Guid guidObject, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType, Guid guidSignature);

		ICrossReferenceNode CreateNode(string stName, IAccessInfo accessInfo, CrossRefSearchType searchType, CrossRefOccurence occurence, CrossReferenceMatchType matchType);

		IAccessInfo2 CreateAccessInfo(ISourcePosition position, AccessFlag access, Guid applicationGuid, Guid messageGuid);

		ISourcePosition CreateSourcePosition(int nProjectHandle, Guid objectGuid, long position, short? offset = null, short length = 0);

		IList<IRelatedSignature> GetRelatedSignatures(ISignature signature, SignatureRelationFlags flags);

		string GetQualifiedNameOfSignature(ISignature signature);
	}
}
