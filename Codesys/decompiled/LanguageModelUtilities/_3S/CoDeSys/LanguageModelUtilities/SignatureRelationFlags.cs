using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[Flags]
	public enum SignatureRelationFlags
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		OverriddenBases = 1,
		[ReleasedEnumMember]
		ImplementedInterfaces = 2,
		[ReleasedEnumMember]
		OverridingSubclasses = 4,
		[ReleasedEnumMember]
		ScopeApplication = 0x100,
		[ReleasedEnumMember]
		ScopeProject = 0x200,
		[ReleasedEnumMember]
		ScopeLibraries = 0x400,
		[ReleasedEnumMember]
		ImplementingInterface = 8
	}
}
