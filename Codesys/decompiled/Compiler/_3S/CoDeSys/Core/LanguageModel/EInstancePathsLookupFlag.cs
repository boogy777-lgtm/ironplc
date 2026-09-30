using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[Flags]
	public enum EInstancePathsLookupFlag
	{
		[ReleasedEnumMember]
		None = 0,
		[ReleasedEnumMember]
		WithNamespace = 1,
		[ReleasedEnumMember]
		WithStackVariables = 2,
		[ReleasedEnumMember]
		WithDerivedFunctionBlocks = 4
	}
}
