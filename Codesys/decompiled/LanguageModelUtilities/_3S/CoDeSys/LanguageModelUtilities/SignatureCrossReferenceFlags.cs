using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[Flags]
	public enum SignatureCrossReferenceFlags
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		DirectCallsOnly = 1,
		[ReleasedEnumMember]
		IncludeOverridenCalls = 2
	}
}
