using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICrossReferenceNode : IMessage
	{
		CrossReferenceScopeType ScopeType { get; }

		string ScopeTypeText { get; }

		string AddressText { get; }

		string NameText { get; }

		string TypeText { get; }

		string Location { get; }

		string AccessText { get; }

		string CommentText { get; }

		string Name { get; }

		IAccessInfo AccessInfo { get; }

		IIdentifierInfo IdentifierInfo { get; }

		[Obsolete("ShadowConflictIdentifierInfo is no longer used, function will return null. Use ShadowingQualifier in ICrossreferenceNode2 instead")]
		IIdentifierInfo ShadowConflictIdentifierInfo { get; }

		ISignature Signature { get; }

		IMetaObject MetaObject { get; }

		IVariable Variable { get; }

		bool Visible { get; }

		CrossRefSearchType SearchType { get; }

		CrossRefOccurence Occurence { get; }

		CrossReferenceMatchType MatchType { get; }
	}
}
