using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	public enum InternalErrorIds
	{
		[ReleasedEnumMember]
		None,
		[ReleasedEnumMember]
		ErrInBlobLink,
		[ReleasedEnumMember]
		ErrInFindObjectsToTypify,
		[ReleasedEnumMember]
		ErrCodeDataLocationConflict,
		[ReleasedEnumMember]
		ErrReLinkError,
		[ReleasedEnumMember]
		ErrInRelocation,
		[ReleasedEnumMember]
		ErrInVirtualFunctionCall
	}
}
