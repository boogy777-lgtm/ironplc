using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[Flags]
	public enum CrossReferenceMatchType
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		Existing = 1,
		[ReleasedEnumMember]
		Shadowed = 2,
		[ReleasedEnumMember]
		[Obsolete("ShadowConflict is no longer used, member won't be used any more . Use Shadowed instead")]
		ShadowConflict = 4,
		[ReleasedEnumMember]
		Unrelated = 8,
		[ReleasedEnumMember]
		ShadowResolutionNotPossible = 0x10
	}
}
